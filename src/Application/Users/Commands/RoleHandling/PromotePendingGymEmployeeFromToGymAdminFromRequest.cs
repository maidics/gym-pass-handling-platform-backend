using FitPass.Application.Common.Extensions;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Models;
using FitPass.Application.Common.Security;
using FitPass.Application.Requests.Commands;
using FitPass.Application.Requests.DTOs;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using FitPass.Domain.Enums;
using FitPass.Domain.Strings;

namespace FitPass.Application.Users.Commands.RoleHandling;


[Authorize(Roles = Roles.AppAdministrator)]
public record PromotePendingGymEmployeeToGymAdminFromRequestCommand(string RequestId) : IRequest<Result>;

public class PromotePendingGymEmployeeToGymAdminFromRequestCommandValidator : AbstractValidator<PromotePendingGymEmployeeToGymAdminFromRequestCommand>
{
    public PromotePendingGymEmployeeToGymAdminFromRequestCommandValidator()
    {
        RuleFor(v => v.RequestId).NotEmptyWithMessage(nameof(PromotePendingGymEmployeeToGymAdminFromRequestCommand.RequestId));
    }
}

public class PromotePendingGymEmployeeToGymAdminFromRequestCommandHandler : IRequestHandler<PromotePendingGymEmployeeToGymAdminFromRequestCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly ISender _sender;
    private readonly IIdentityService _identityService;
    private readonly TimeProvider _timeProvider;

    public PromotePendingGymEmployeeToGymAdminFromRequestCommandHandler(
        IApplicationDbContext context,
        ISender sender,
        IIdentityService identityService,
        TimeProvider timeProvider)
    {
        _context = context;
        _sender = sender;
        _identityService = identityService;
        _timeProvider = timeProvider;
    }

    public async Task<Result> Handle(PromotePendingGymEmployeeToGymAdminFromRequestCommand command, CancellationToken cancellationToken)
    {
        var request = await _context.Requests.FindAsync(command.RequestId);

        if (request is null)
        {
            return Result.NotFound(nameof(Request));
        }

        if (request.Status != RequestStatus.Submitted)
        {
            return Result.Forbidden();
        }

        if (request.Type != RequestType.GymAdminPromotion)
        {
            return Result.BusinessRuleViolation("Request is not of GymAdminPromotion type.");
        }

        var deserializationResult = await _sender.Send(new DeserializeRequestPayloadCommand<GymAdminPromotionDto>(request));

        if (!deserializationResult.Succeeded)
        {
            request.Status = RequestStatus.Error;
            request.Error = string.Join(", ", deserializationResult.Errors);

            await _context.SaveChangesAsync();

            return Result.InternalError("Failed to retrieve details from request.");
        }

        var promotionDto = deserializationResult.Value;

        if (!await _identityService.DoesUserExist(promotionDto.UserIdToNominate))
        {
            return Result.NotFound("User to promote");
        }

        if (!await _identityService.IsInRoleAsync(promotionDto.UserIdToNominate, Roles.PendingGymEmployee))
        {
            return Result.BusinessRuleViolation("User is not a PendingGymEmployee.");
        }

        var gym = await _context.Gyms
            .AsNoTracking()
            .FirstOrDefaultAsync(g => g.Id == promotionDto.GymId);

        if (gym is null)
        {
            return Result.NotFound(nameof(Gym));
        }

        using var transaction = await _context.BeginTransactionAsync();

        try
        {
            var demotionResult = await _identityService.RemoveFromRoleAsync(promotionDto.UserIdToNominate, Roles.PendingGymEmployee);

            if (!demotionResult.Succeeded)
            {
                await transaction.RollbackAsync();

                throw new Exception(ErrorMessages.FailedToHandleRole(Roles.PendingGymEmployee, false, demotionResult.Errors));
            }

            var promotionResult = await _identityService.AddToRoleAsync(promotionDto.UserIdToNominate, Roles.GymAdministrator);

            if (!promotionResult.Succeeded)
            {
                await transaction.RollbackAsync();

                throw new Exception(ErrorMessages.FailedToHandleRole(Roles.GymAdministrator, true, demotionResult.Errors));
            }

            var gymEmployment = new GymEmployment
            {
                UserId = promotionDto.UserIdToNominate,
                GymId = promotionDto.GymId,
                Role = Roles.GymAdministrator,
                EscalationEmail = promotionDto.EscalationEmail,
                EmploymentStart = _timeProvider.GetUtcNow()
            };

            await _context.GymEmployments.AddAsync(gymEmployment);

            request.Status = RequestStatus.Completed;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Result.Success();
        } catch
        {
            await transaction.RollbackAsync();

            throw;
        }
    }
}
