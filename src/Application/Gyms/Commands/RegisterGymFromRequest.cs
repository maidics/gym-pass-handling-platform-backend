using FitPass.Application.Common.Extensions;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Models;
using FitPass.Application.Common.Security;
using FitPass.Application.Gyms.DTOs;
using FitPass.Application.Requests.Commands;
using FitPass.Application.Requests.DTOs;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using FitPass.Domain.Enums;
using FitPass.Domain.Strings;

namespace FitPass.Application.Gyms.Commands;

[Authorize(Roles = Roles.AppAdministrator)]
public record RegisterGymFromRequestCommand(string RequestId) : IRequest<Result<GymDto>>;

public class RegisterGymFromRequestCommandValidator : AbstractValidator<RegisterGymFromRequestCommand>
{
    public RegisterGymFromRequestCommandValidator()
    {
        RuleFor(v => v.RequestId).NotEmptyWithMessage(nameof(RegisterGymFromRequestCommand.RequestId));
    }
}

public class RegisterGymFromRequestCommandHandler : IRequestHandler<RegisterGymFromRequestCommand, Result<GymDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly IIdentityService _identityService;
    private readonly ISender _sender;
    private readonly TimeProvider _timeProvider;

    public RegisterGymFromRequestCommandHandler(
        IApplicationDbContext context,
        IUser user,
        IIdentityService identityService,
        ISender sender,
        TimeProvider timeProvider)
    {
        _context = context;
        _user = user;
        _identityService = identityService;
        _sender = sender;
        _timeProvider = timeProvider;
    }
    
    public async Task<Result<GymDto>> Handle(RegisterGymFromRequestCommand command, CancellationToken cancellationToken)
    {
        var request = await _context
            .Requests
            .FindAsync(command.RequestId);

        if (request is null)
        {
            return Result.NotFound(nameof(Request));
        }

        if (request.Status != RequestStatus.Submitted)
        {
            return Result.Forbidden("Request is no longer open.");
        }

        if (request.Type != RequestType.GymCreation)
        {
            return Result.BusinessRuleViolation("Request is not of GymCreation type.");
        }

        if (request.CreatedBy is null)
        {
            request.Status = RequestStatus.Error;
            request.Error = "Request creator is empty.";

            await _context.SaveChangesAsync();

            return Result.InternalError("Request creator is empty.");
        } else
        {
            if (!await _identityService.DoesUserExist(request.CreatedBy))
            {
                request.Status = RequestStatus.Error;
                request.Error = "Request creator not found.";
                await _context.SaveChangesAsync();
                return Result.NotFound("Request creator not found.");
            }

            if (!await _identityService.IsInRoleAsync(request.CreatedBy, Roles.PendingGymEmployee))
            {
                request.Status = RequestStatus.Error;
                request.Error = "Request creator is no longer eligible for request completion.";

                await _context.SaveChangesAsync();

                return Result.BusinessRuleViolation("User is no longer a PendingGymEmployee.");
            }
        }

        var deserializationResult = await _sender.Send(new DeserializeRequestPayloadCommand<CreateGymDto>(request));

        if (!deserializationResult.Succeeded)
        {
            request.Status = RequestStatus.Error;
            request.Error = "Failed to deserialize payload.";

            await _context.SaveChangesAsync();

            return Result.InternalError("Failed to retrieve gym details from request.");
        }

        var createGymDto = deserializationResult.Value;

        var existingGym = await _context
                .Gyms
                .AsNoTracking()
                .FirstOrDefaultAsync(g => g.Name == createGymDto.GymName); //TODO: make this more robust

        if (existingGym is not null)
        {
            return Result.Conflict("Gym name");
        }

        using var transaction = await _context.BeginTransactionAsync();

        try
        {
            var gym = new Gym
            {
                Name = createGymDto.GymName,
                Address = createGymDto.GymAddress,
                Status = createGymDto.GymStatus,
                Tier = createGymDto.GymTier,
            };

            await _context.Gyms.AddAsync(gym);

            var demotionResult = await _identityService.RemoveFromRoleAsync(request.CreatedBy, Roles.PendingGymEmployee);

            if (!demotionResult.Succeeded)
            {
                await transaction.RollbackAsync();

                request.Status = RequestStatus.Error;
                request.Error = $"Failed to remove user from {Roles.PendingGymEmployee} role.";

                await _context.SaveChangesAsync();

                throw new Exception(ErrorMessages.FailedToHandleRole(Roles.PendingGymEmployee, false, demotionResult.Errors));
            }

            var promotionResult = await _identityService.AddToRoleAsync(request.CreatedBy, Roles.GymAdministrator);

            if (!promotionResult.Succeeded)
            {
                await transaction.RollbackAsync();

                request.Status = RequestStatus.Error;
                request.Error = $"Failed to add user to {Roles.GymAdministrator} role.";

                await _context.SaveChangesAsync();

                throw new Exception(ErrorMessages.FailedToHandleRole(Roles.GymAdministrator, true, promotionResult.Errors));
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
