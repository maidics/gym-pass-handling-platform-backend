using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Models;
using FitPass.Application.Common.Security;
using FitPass.Application.Extensions;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
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

    public PromotePendingGymEmployeeToGymStaffRoleCommandHandler(
        IIdentityService identityService,
        ILogger<PromotePendingGymEmployeeToGymStaffRoleCommand> logger,
        IUser user,
        IApplicationDbContext context)
    {
        _identityService = identityService;
        _logger = logger;
        _user = user;
        _context = context;
    }
    public async Task Handle(PromotePendingGymEmployeeToGymStaffRoleCommand command, CancellationToken cancellationToken)
    {
        var promoterGymAdminEmployment = await _context
            .GymEmployments
            .AsNoTracking()
            .FirstOrDefaultAsync(ge => ge.ApplicationUserId == _user.Id);

        if (promoterGymAdminEmployment == null)
        {
            _logger.LogError("Promoter GymAdmin (currently logged in) not found ({GymAdminId})", _user.Id);
            throw new UnauthorizedAccessException();
        }

        using var transaction = await _context.BeginTransactionAsync();
        
        try
        {
            var demotionResult = await _identityService.RemoveFromRoleAsync(command.UserId, Roles.PendingGymEmployee);

            if (!demotionResult.Succeeded)
            {
                _logger.LogError(
                        "Failed to remove PendingGymEmployee ({UserId}) from {Role} role. Result: {Result}", 
                        command.UserId, 
                        Roles.PendingGymEmployee, 
                        demotionResult
                    );

                await transaction.RollbackAsync();

                if (demotionResult.IsUserNotFoundFailure())
                {
                    throw new NotFoundException(command.UserId, "User");
                }

                throw new Exception($"Failed to remove PendingGymEmployee from their role. Errors: {string.Join(", ", demotionResult.Errors)}");
            }

            var promotionResult = await _identityService.AddToRoleAsync(command.UserId, Roles.GymStaff);

            if (!promotionResult.Succeeded)
            {
                _logger.LogError(
                        "Failed to add previous PendingGymEmployee ({UserId}) to {Role} role. Result: {Result}",
                        command.UserId,
                        Roles.GymStaff,
                        promotionResult
                    );

                await transaction.RollbackAsync();

                if (promotionResult.IsUserNotFoundFailure())
                {
                    _logger.LogError("Previously removed PendingGymEmployee not found when adding them to role.");
                    throw new NotFoundException(command.UserId, "User");
                }

                throw new Exception($"Failed to add previous PendingGymEmployee to GymStaff role. Errors: {string.Join(", ", promotionResult.Errors)}");
            }

            var gymEmployment = new GymEmployment
            {
                ApplicationUserId = command.UserId,
                GymId = promoterGymAdminEmployment.GymId,
                Role = Roles.GymStaff
            };

            await _context.GymEmployments.AddAsync(gymEmployment);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

        } catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception caught.");

            await transaction.RollbackAsync();

            throw;
        }
    }
}
