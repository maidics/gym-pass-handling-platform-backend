using FitPass.Application.Common.Extensions;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Models;
using FitPass.Application.Common.Security;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using FitPass.Domain.Strings;

namespace FitPass.Application.Users.Commands.RoleHandling;

[Authorize(Roles = Roles.GymAdministrator)]
public record DemoteGymStaffToPendingGymEmployeeCommand(string UserId) : IRequest<Result>;

public class DemoteGymStaffToPendingGymEmployeeCommandValidator : AbstractValidator<DemoteGymStaffToPendingGymEmployeeCommand>
{
    public DemoteGymStaffToPendingGymEmployeeCommandValidator()
    {
        RuleFor(v => v.UserId).NotEmptyWithMessage(nameof(DemoteGymStaffToPendingGymEmployeeCommand.UserId));
    }
}

public class DemoteGymStaffToPendingGymEmployeeCommandHandler : IRequestHandler<DemoteGymStaffToPendingGymEmployeeCommand, Result>
{
    private readonly IIdentityService _identityService;
    private readonly IUser _user;
    private readonly IApplicationDbContext _context;

    public DemoteGymStaffToPendingGymEmployeeCommandHandler(
        IIdentityService identityService,
        IUser user,
        IApplicationDbContext context)
    {
        _identityService = identityService;
        _user = user;
        _context = context;
    }
    public async Task<Result> Handle(DemoteGymStaffToPendingGymEmployeeCommand command, CancellationToken cancellationToken)
    {
        var demoterGymEmployment = await _context
            .GymEmployments
            .AsNoTracking()
            .FirstOrDefaultAsync(ge => ge.UserId == _user.Id);

        Guard.Against.NullParameterRelatedToCurrentUser(demoterGymEmployment, nameof(GymEmployment), _user.Id);

        var gymStaffGymEmployment = await _context
            .GymEmployments
            .FirstOrDefaultAsync(ge => ge.UserId == command.UserId);

        if (gymStaffGymEmployment is null)
        {
            return Result.NotFound("GymStaff user's gym employment");
        }

        if (gymStaffGymEmployment.GymId != demoterGymEmployment.GymId || gymStaffGymEmployment.Role != Roles.GymStaff)
        {
            return Result.Forbidden();
        }

        using var transaction = await _context.BeginTransactionAsync();

        try
        {
            var demotionResult = await _identityService.RemoveFromRoleAsync(command.UserId, Roles.GymStaff);

            if (!demotionResult.Succeeded)
            {
                await transaction.RollbackAsync();

                throw new Exception(ErrorMessages.FailedToHandleRole(Roles.PendingGymEmployee, false, demotionResult.Errors));
            }

            var promotionResult = await _identityService.AddToRoleAsync(command.UserId, Roles.PendingGymEmployee);

            if (!promotionResult.Succeeded) //cannot be user not found if demotion succeeds
            {
                await transaction.RollbackAsync();

                throw new Exception(ErrorMessages.FailedToHandleRole(Roles.PendingGymEmployee, true, promotionResult.Errors));
            }

            _context.GymEmployments.Remove(gymStaffGymEmployment);

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
