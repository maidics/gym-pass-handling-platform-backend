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
public record PromotePendingGymEmployeeToGymStaffRoleCommand (string UserId) : IRequest;

public class PromotePendingGymEmployeeToGymStaffRoleCommandValidator : AbstractValidator<PromotePendingGymEmployeeToGymStaffRoleCommand>
{
    public PromotePendingGymEmployeeToGymStaffRoleCommandValidator()
    {
        RuleFor(v => v.UserId).NotEmptyWithMessage(nameof(PromotePendingGymEmployeeToGymStaffRoleCommand.UserId));
    }
}

public class PromotePendingGymEmployeeToGymStaffRoleCommandHandler : IRequestHandler<PromotePendingGymEmployeeToGymStaffRoleCommand>
{
    private readonly IIdentityService _identityService;
    private readonly ILogger<PromotePendingGymEmployeeToGymStaffRoleCommand> _logger;
    private readonly IUser _user;
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;

    public PromotePendingGymEmployeeToGymStaffRoleCommandHandler(
        IIdentityService identityService,
        ILogger<PromotePendingGymEmployeeToGymStaffRoleCommand> logger,
        IUser user,
        IApplicationDbContext context,
        TimeProvider timeProvider)
    {
        _identityService = identityService;
        _logger = logger;
        _user = user;
        _context = context;
        _timeProvider = timeProvider;
    }
    public async Task Handle(PromotePendingGymEmployeeToGymStaffRoleCommand command, CancellationToken cancellationToken)
    {
        var promoterGymAdminEmployment = await _context
            .GymEmployments
            .AsNoTracking()
            .FirstOrDefaultAsync(ge => ge.UserId == _user.Id);

        if (promoterGymAdminEmployment == null)
        {
            LogCriticalMessages.AuthenticatedUserRelatedEntityNotFound(_logger, _user.Roles, _user.Id, nameof(GymEmployment));
            throw new Exception(ErrorMessages.AuthenticatedUserRelatedEntityNotFound(nameof(GymEmployment)));
        }

        if (!await _identityService.DoesUserExist(command.UserId))
        {
            throw new NotFoundException(command.UserId, "User");
        }

        if (!await _identityService.IsInRoleAsync(command.UserId, Roles.PendingGymEmployee))
        {
            throw new ForbiddenAccessException();
        }

        using var transaction = await _context.BeginTransactionAsync();
        
        try
        {
            var demotionResult = await _identityService.RemoveFromRoleAsync(command.UserId, Roles.PendingGymEmployee);

            if (!demotionResult.Succeeded)
            {
                await transaction.RollbackAsync();

                if (demotionResult.IsResultFailureWithOneErrorMessage(ErrorMessages.UserNotFound()))
                {
                    throw new NotFoundException(command.UserId, "User");
                } else
                {
                    LogErrorMessages.IdentityServiceMethodFailed(_logger, nameof(IIdentityService.RemoveFromRoleAsync), [Roles.PendingGymEmployee], command.UserId, demotionResult);
                    
                    throw new Exception(ErrorMessages.FailedToHandleRole(Roles.PendingGymEmployee, false, demotionResult.Errors));
                }
            }

            var promotionResult = await _identityService.AddToRoleAsync(command.UserId, Roles.GymStaff);

            if (!promotionResult.Succeeded) //cannot be user not found if demotion succeeds
            {
                await transaction.RollbackAsync();

                LogErrorMessages.IdentityServiceMethodFailed(_logger, nameof(IIdentityService.AddToRoleAsync), [Roles.GymStaff], command.UserId, promotionResult);

                throw new Exception(ErrorMessages.FailedToHandleRole(Roles.GymStaff, true, promotionResult.Errors));
            }

            var gymEmployment = new GymEmployment
            {
                UserId = command.UserId,
                GymId = promoterGymAdminEmployment.GymId,
                Role = Roles.GymStaff,
                EmploymentStart = _timeProvider.GetUtcNow()
            };

            await _context.GymEmployments.AddAsync(gymEmployment);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        } catch (Exception ex)
        {
            LogErrorMessages.UnhandledExceptionCaught(_logger, nameof(PromotePendingGymEmployeeToGymStaffRoleCommandHandler), ex);

            await transaction.RollbackAsync();

            throw;
        }
    }
}
