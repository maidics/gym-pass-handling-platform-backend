using FitPass.Application.Common.Extensions;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Models;
using FitPass.Application.Common.Security;
using FitPass.Application.Gyms.DTOs;
using FitPass.Application.Requests.DTOs;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using FitPass.Domain.Enums;
using FitPass.Application.Common.Resources;

namespace FitPass.Application.Requests.Commands.Fulfill;

[Authorize(Roles = Roles.AppAdministrator)]
public record RegisterGymFromRequestCommand(string RequestId) : IRequest<Result<GymDto>>;

public class RegisterGymFromRequestCommandValidator : AbstractValidator<RegisterGymFromRequestCommand>
{
    public RegisterGymFromRequestCommandValidator(ILocalizer localizer)
    {
        RuleFor(v => v.RequestId)
            .PropertyOfEntityNotEmptyWithMessageLocalized(localizer, nameof(SharedResource.Id), nameof(SharedResource.Request));
    }
}

public class RegisterGymFromRequestCommandHandler : IRequestHandler<RegisterGymFromRequestCommand, Result<GymDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _identityService;
    private readonly ISender _sender;
    private readonly TimeProvider _timeProvider;
    private readonly ILocalizer _localizer;

    public RegisterGymFromRequestCommandHandler(
        IApplicationDbContext context,
        IIdentityService identityService,
        ISender sender,
        TimeProvider timeProvider,
        ILocalizer localizer)
    {
        _context = context;
        _identityService = identityService;
        _sender = sender;
        _timeProvider = timeProvider;
        _localizer = localizer;
    }
    
    public async Task<Result<GymDto>> Handle(RegisterGymFromRequestCommand command, CancellationToken cancellationToken)
    {
        var request = await _context
            .Requests
            .FindAsync(command.RequestId);

        if (request is null)
        {
            return Result.NotFound(_localizer.GetNotFound(nameof(SharedResource.Request)));
        }

        if (request.Status != RequestStatus.Submitted)
        {
            return Result.Forbidden(_localizer.Get(nameof(SharedResource.RequestIsNotOpen)));
        }

        if (request.Type != RequestType.GymCreation)
        {
            return Result.BusinessRuleViolation(_localizer.Get(nameof(SharedResource.ActionIsApplicableForRequestType)));
        }

        if (request.CreatedBy is null)
        {
            request.Status = RequestStatus.Error;
            request.Error = "Request creator is empty.";

            await _context.SaveChangesAsync();

            return Result.InternalError(_localizer.Get(nameof(SharedResource.RequestHandlingError)));
        } else
        {
            if (!await _identityService.DoesUserExist(request.CreatedBy))
            {
                request.Status = RequestStatus.Error;
                request.Error = "Request creator not found.";
                
                await _context.SaveChangesAsync();
                
                return Result.NotFound(_localizer.Get(nameof(SharedResource.RequestHandlingError)));
            }

            if (!await _identityService.IsInRoleAsync(request.CreatedBy, Roles.PendingGymEmployee))
            {
                request.Status = RequestStatus.Error;
                request.Error = "Request creator is no longer eligible for request completion.";

                await _context.SaveChangesAsync();

                return Result.BusinessRuleViolation(_localizer.Get(nameof(SharedResource.CannotPerformActionOnRoleType)));
            }
        }

        var deserializationResult = await _sender.Send(new DeserializeRequestPayloadCommand<CreateGymDto>(request));

        if (!deserializationResult.Succeeded)
        {
            request.Status = RequestStatus.Error;
            request.Error = "Failed to deserialize payload.";

            await _context.SaveChangesAsync();

            return Result.InternalError(_localizer.Get(nameof(SharedResource.RequestHandlingError)));
        }

        var createGymDto = deserializationResult.Value;

        var existingGym = await _context
                .Gyms
                .AsNoTracking()
                .FirstOrDefaultAsync(g => g.Name == createGymDto.Name); //TODO: make this more robust

        if (existingGym is not null)
        {
            return Result.Conflict(
                _localizer.Get(nameof(SharedResource.Conflict), 
                    _localizer.GetWithParamsLocalized(nameof(SharedResource.Name), nameof(SharedResource.Gym))));
        }

        using var transaction = await _context.BeginTransactionAsync();

        try
        {
            var gym = new Gym
            {
                Name = createGymDto.Name,
                Address = createGymDto.Address,
                Status = createGymDto.Status,
                Tier = createGymDto.Tier,
            };

            await _context.Gyms.AddAsync(gym);

            var demotionResult = await _identityService.RemoveFromRoleAsync(request.CreatedBy, Roles.PendingGymEmployee);

            if (!demotionResult.Succeeded)
            {
                await transaction.RollbackAsync();

                request.Status = RequestStatus.Error;
                request.Error = $"Failed to remove user from {Roles.PendingGymEmployee} role.";

                await _context.SaveChangesAsync();

                throw new Exception($"Failed to remove user from their role. Result: {demotionResult}.");
            }

            var promotionResult = await _identityService.AddToRoleAsync(request.CreatedBy, Roles.GymAdministrator);

            if (!promotionResult.Succeeded)
            {
                await transaction.RollbackAsync();

                request.Status = RequestStatus.Error;
                request.Error = $"Failed to add user to {Roles.GymAdministrator} role.";

                await _context.SaveChangesAsync();

                throw new Exception($"Failed to add user to role. Result: {promotionResult}");
            }

            var gymEmployment = new GymEmployment
            {
                UserId = request.CreatedBy,
                GymId = gym.Id,
                Role = Roles.GymAdministrator,
                EscalationEmail = createGymDto.EscalationEmail,
                EmploymentStart = _timeProvider.GetUtcNow()
            };

            await _context.GymEmployments.AddAsync(gymEmployment);

            request.Status = RequestStatus.Completed;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Result.Success(gym.MapToDto());
        } catch
        {
            await transaction.RollbackAsync();

            throw;
        }
    }
}
