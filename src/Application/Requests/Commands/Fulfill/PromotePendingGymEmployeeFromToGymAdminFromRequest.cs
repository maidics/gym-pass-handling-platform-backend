using FitPass.Application.Common.Extensions;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Models;
using FitPass.Application.Common.Resources;
using FitPass.Application.Common.Security;
using FitPass.Application.Requests.DTOs;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using FitPass.Domain.Enums;

namespace FitPass.Application.Requests.Commands.Fulfill;

[Authorize(Roles = Roles.AppAdministrator)]
public record PromotePendingGymEmployeeToGymAdminFromRequestCommand(string RequestId)
    : IRequest<Result>;

public class PromotePendingGymEmployeeToGymAdminFromRequestCommandValidator
    : AbstractValidator<PromotePendingGymEmployeeToGymAdminFromRequestCommand>
{
    public PromotePendingGymEmployeeToGymAdminFromRequestCommandValidator(ILocalizer localizer)
    {
        RuleFor(v => v.RequestId)
            .PropertyOfEntityNotEmptyWithMessageLocalized(
                localizer,
                nameof(SharedResource.Id),
                nameof(SharedResource.Request)
            );
    }
}

public class PromotePendingGymEmployeeToGymAdminFromRequestCommandHandler
    : IRequestHandler<PromotePendingGymEmployeeToGymAdminFromRequestCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly ISender _sender;
    private readonly IIdentityService _identityService;
    private readonly ILocalizer _localizer;
    private readonly TimeProvider _timeProvider;

    public PromotePendingGymEmployeeToGymAdminFromRequestCommandHandler(
        IApplicationDbContext context,
        ISender sender,
        IIdentityService identityService,
        ILocalizer localizer,
        TimeProvider timeProvider
    )
    {
        _context = context;
        _sender = sender;
        _identityService = identityService;
        _localizer = localizer;
        _timeProvider = timeProvider;
    }

    public async Task<Result> Handle(
        PromotePendingGymEmployeeToGymAdminFromRequestCommand command,
        CancellationToken cancellationToken
    )
    {
        var request = await _context.Requests.FindAsync([command.RequestId], cancellationToken);

        if (request is null)
        {
            return Result.NotFound(nameof(Request));
        }

        if (request.Status != RequestStatus.Submitted)
        {
            return Result.Forbidden(_localizer.Get(nameof(SharedResource.RequestIsNotOpen)));
        }

        if (request.Type != RequestType.GymAdminPromotion)
        {
            return Result.BusinessRuleViolation(
                _localizer.Get(nameof(SharedResource.ActionIsApplicableForRequestType))
            );
        }

        var deserializationResult = await _sender.Send(
            new DeserializeRequestPayloadCommand<GymAdminPromotionDto>(request),
            cancellationToken
        );

        if (!deserializationResult.Succeeded)
        {
            request.Status = RequestStatus.Error;
            request.Error = deserializationResult.Message;

            await _context.SaveChangesAsync(cancellationToken);

            return Result.InternalError(
                _localizer.Get(nameof(SharedResource.RequestHandlingError))
            );
        }

        var promotionDto = deserializationResult.Value;

        var pendingGymEmployeeId = await _identityService.GetUserIdByEmailAsync(
            promotionDto.PendingGymEmployeeEmail
        );

        if (pendingGymEmployeeId is null)
        {
            return Result.NotFound(_localizer.GetNotFound(nameof(SharedResource.User)));
        }

        if (!await _identityService.IsInRoleAsync(pendingGymEmployeeId, Roles.PendingGymEmployee))
        {
            return Result.BusinessRuleViolation(
                _localizer.Get(nameof(SharedResource.CannotPerformActionOnRoleType))
            );
        }

        var gym = await _context
            .Gyms.AsNoTracking()
            .FirstOrDefaultAsync(g => g.Id == promotionDto.GymId, cancellationToken);

        if (gym is null)
        {
            return Result.NotFound(_localizer.GetNotFound(nameof(SharedResource.Gym)));
        }

        await using var transaction = await _context.BeginTransactionAsync(cancellationToken);

        try
        {
            var demotionResult = await _identityService.RemoveFromRoleAsync(
                pendingGymEmployeeId,
                Roles.PendingGymEmployee
            );

            if (!demotionResult.Succeeded)
            {
                await transaction.RollbackAsync(cancellationToken);

                throw new Exception(
                    $"Failed to demote user from their user role. Result {demotionResult}."
                );
            }

            var promotionResult = await _identityService.AddToRoleAsync(
                pendingGymEmployeeId,
                Roles.GymAdministrator
            );

            if (!promotionResult.Succeeded)
            {
                await transaction.RollbackAsync(cancellationToken);

                throw new Exception($"Failed to promote user to role. Result: {promotionResult}");
            }

            var gymEmployment = new GymEmployment
            {
                UserId = pendingGymEmployeeId,
                GymId = promotionDto.GymId,
                Role = Roles.GymAdministrator,
                SupervisorEmail = promotionDto.SupervisorEmail,
                CreatedOn = _timeProvider.GetUtcNow(),
            };

            await _context.GymEmployments.AddAsync(gymEmployment, cancellationToken);

            request.Status = RequestStatus.Approved;

            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return Result.Success();
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);

            throw;
        }
    }
}
