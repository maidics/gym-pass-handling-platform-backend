using FitPass.Application.Common.Extensions;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Models;
using FitPass.Application.Common.Resources;
using FitPass.Application.Common.Security;
using FitPass.Application.Gyms.DTOs;
using FitPass.Application.Requests.DTOs;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using FitPass.Domain.Enums;

namespace FitPass.Application.Requests.Commands.Fulfill;

[Authorize(Roles = Roles.AppAdministrator)]
public record RegisterGymFromRequestCommand(string RequestId) : IRequest<Result<GymDto>>;

public class RegisterGymFromRequestCommandValidator
    : AbstractValidator<RegisterGymFromRequestCommand>
{
    public RegisterGymFromRequestCommandValidator(ILocalizer localizer)
    {
        RuleFor(v => v.RequestId)
            .PropertyOfEntityNotEmptyWithMessageLocalized(
                localizer,
                nameof(SharedResource.Id),
                nameof(SharedResource.Request)
            );
    }
}

public class RegisterGymFromRequestCommandHandler
    : IRequestHandler<RegisterGymFromRequestCommand, Result<GymDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _identityService;
    private readonly ISender _sender;
    private readonly ILocalizer _localizer;
    private readonly TimeProvider _timeProvider;

    public RegisterGymFromRequestCommandHandler(
        IApplicationDbContext context,
        IIdentityService identityService,
        ISender sender,
        ILocalizer localizer,
        TimeProvider timeProvider
    )
    {
        _context = context;
        _identityService = identityService;
        _sender = sender;
        _localizer = localizer;
        _timeProvider = timeProvider;
    }

    public async Task<Result<GymDto>> Handle(
        RegisterGymFromRequestCommand command,
        CancellationToken cancellationToken
    )
    {
        var request = await _context.Requests.FindAsync([command.RequestId], cancellationToken);

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
            return Result.BusinessRuleViolation(
                _localizer.Get(nameof(SharedResource.ActionIsApplicableForRequestType))
            );
        }

        if (request.CreatedBy is null)
        {
            return Result.InternalError(
                _localizer.Get(nameof(SharedResource.RequestHandlingError))
            );
        }

        if (!await _identityService.DoesUserExist(request.CreatedBy))
        {
            return Result.NotFound(_localizer.Get(nameof(SharedResource.RequestHandlingError)));
        }

        if (!await _identityService.IsInRoleAsync(request.CreatedBy, Roles.PendingGymEmployee))
        {
            return Result.BusinessRuleViolation(
                _localizer.Get(nameof(SharedResource.CannotPerformActionOnRoleType))
            );
        }

        var deserializationResult = await _sender.Send(
            new DeserializeRequestPayloadCommand<CreateGymDto>(request),
            cancellationToken
        );

        if (!deserializationResult.Succeeded)
        {
            request.Status = RequestStatus.Error;
            request.Error = "Failed to deserialize payload.";

            await _context.SaveChangesAsync(cancellationToken);

            return Result.InternalError(
                _localizer.Get(nameof(SharedResource.RequestHandlingError))
            );
        }

        var createGymDto = deserializationResult.Value;

        var existingGym = await _context
            .Gyms.AsNoTracking()
            .FirstOrDefaultAsync(g => g.Name == createGymDto.Name, cancellationToken);

        if (existingGym is not null)
        {
            return Result.Conflict(
                _localizer.Get(
                    nameof(SharedResource.Conflict),
                    _localizer.GetWithParamsLocalized(
                        nameof(SharedResource.Name),
                        nameof(SharedResource.Gym)
                    )
                )
            );
        }

        await using var transaction = await _context.BeginTransactionAsync(cancellationToken);

        try
        {
            var gym = new Gym
            {
                Name = createGymDto.Name,
                Address = createGymDto.Address,
                Status = createGymDto.Status,
                Tier = createGymDto.Tier,
            };

            await _context.Gyms.AddAsync(gym, cancellationToken);

            var demotionResult = await _identityService.RemoveFromRoleAsync(
                request.CreatedBy,
                Roles.PendingGymEmployee
            );

            if (!demotionResult.Succeeded)
            {
                throw new Exception(
                    $"Failed to remove user from their role. Result: {demotionResult}."
                );
            }

            var promotionResult = await _identityService.AddToRoleAsync(
                request.CreatedBy,
                Roles.GymAdministrator
            );

            if (!promotionResult.Succeeded)
            {
                throw new Exception($"Failed to add user to role. Result: {promotionResult}");
            }

            var gymEmployment = new GymEmployment
            {
                UserId = request.CreatedBy,
                GymId = gym.Id,
                Role = Roles.GymAdministrator,
                SupervisorEmail = createGymDto.SupervisorEmail,
                CreatedOn = _timeProvider.GetUtcNow(),
            };

            await _context.GymEmployments.AddAsync(gymEmployment, cancellationToken);

            request.Status = RequestStatus.Approved;

            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return Result.Success(gym.MapToDto());
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);

            throw;
        }
    }
}
