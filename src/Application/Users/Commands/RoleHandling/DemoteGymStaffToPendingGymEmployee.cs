using FitPass.Application.Common.Exceptions;
using FitPass.Application.Common.Extensions;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Logging;
using FitPass.Application.Common.Security;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using FitPass.Domain.Strings;
using Microsoft.Extensions.Logging;

namespace FitPass.Application.ApplicationUsers.Commands.RoleHandling;

[Authorize(Roles = Roles.GymAdministrator)]
public record DemoteGymStaffToPendingGymEmployeeCommand(string UserId) : IRequest;

public class DemoteGymStaffToPendingGymEmployeeCommandValidator : AbstractValidator<DemoteGymStaffToPendingGymEmployeeCommand>
{
    public DemoteGymStaffToPendingGymEmployeeCommandValidator()
    {
        RuleFor(v => v.UserId).NotEmptyWithMessage(nameof(DemoteGymStaffToPendingGymEmployeeCommand.UserId));
    }
}

public class DemoteGymStaffToPendingGymEmployeeCommandHandler : IRequestHandler<DemoteGymStaffToPendingGymEmployeeCommand>
{
    private readonly IIdentityService _identityService;
    private readonly ILogger<DemoteGymStaffToPendingGymEmployeeCommandHandler> _logger;
    private readonly IUser _user;
    private readonly IApplicationDbContext _context;

    public DemoteGymStaffToPendingGymEmployeeCommandHandler(
        IIdentityService identityService,
        ILogger<DemoteGymStaffToPendingGymEmployeeCommandHandler> logger,
        IUser user,
        IApplicationDbContext context)
    {
        _identityService = identityService;
        _logger = logger;
        _user = user;
        _context = context;
    }
    public async Task Handle(DemoteGymStaffToPendingGymEmployeeCommand command, CancellationToken cancellationToken)
    {
        var demoterGymAdminEmployment = await _context
            .GymEmployments
            .AsNoTracking()
            .FirstOrDefaultAsync(ge => ge.UserId == _user.Id);

        if (demoterGymAdminEmployment == null)
        {
            LogCriticalMessages.AuthenticatedUserRelatedEntityNotFound(_logger, _user.Roles, _user.Id, nameof(GymEmployment));
            throw new Exception(ErrorMessages.AuthenticatedUserRelatedEntityNotFound(nameof(GymEmployment)));
        }

        var gymStaffGymEmployment = await _context
            .GymEmployments
            .FirstOrDefaultAsync(ge => ge.UserId == command.UserId);

        Guard.Against.NotFound(command.UserId, gymStaffGymEmployment, "GymEmployment");

        if (gymStaffGymEmployment.GymId != demoterGymAdminEmployment.GymId || gymStaffGymEmployment.Role != Roles.GymStaff)
        {
            throw new ForbiddenAccessException();
        }

        using var transaction = await _context.BeginTransactionAsync();

        try
        {
            var demotionResult = await _identityService.RemoveFromRoleAsync(command.UserId, Roles.GymStaff);

            if (!demotionResult.Succeeded)
            {
                await transaction.RollbackAsync();

                if (demotionResult.IsResultFailureWithOneErrorMessage(ErrorMessages.UserNotFound()))
                {
                    LogCriticalMessages.FailedToFindGymEmployeeButHasGymEmployment(_logger, [Roles.GymStaff], command.UserId, gymStaffGymEmployment, null);

                    throw new Exception("Gym staff related GymEmployment not found.");
                } else
                {
                    LogErrorMessages.IdentityServiceMethodFailed(_logger, nameof(IIdentityService.RemoveFromRoleAsync), [Roles.GymStaff], command.UserId, demotionResult);

                    throw new Exception(ErrorMessages.FailedToHandleRole(Roles.GymStaff, false, demotionResult.Errors));
                }
            }

            var promotionResult = await _identityService.AddToRoleAsync(command.UserId, Roles.PendingGymEmployee);

            if (!promotionResult.Succeeded) //cannot be user not found if demotion succeeds
            {
                await transaction.RollbackAsync();

                LogErrorMessages.IdentityServiceMethodFailed(_logger, nameof(IIdentityService.AddToRoleAsync), [Roles.PendingGymEmployee], command.UserId, promotionResult);

                throw new Exception(ErrorMessages.FailedToHandleRole(Roles.PendingGymEmployee, true, promotionResult.Errors));
            }

            _context.GymEmployments.Remove(gymStaffGymEmployment);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        } catch (Exception ex)
        {
            LogErrorMessages.UnhandledExceptionCaught(_logger, nameof(DemoteGymStaffToPendingGymEmployeeCommandHandler), ex);

            await transaction.RollbackAsync();

            throw;
        }
    }
}
