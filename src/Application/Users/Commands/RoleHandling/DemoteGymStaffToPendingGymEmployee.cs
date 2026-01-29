using FitPass.Application.Common.Extensions;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Models;
using FitPass.Application.Common.Security;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using FitPass.Application.Common.Resources;

namespace FitPass.Application.Users.Commands.RoleHandling;

[Authorize(Roles = Roles.GymAdministrator)]
public record DemoteGymStaffToPendingGymEmployeeCommand(string UserId) : IRequest<Result>;

public class DemoteGymStaffToPendingGymEmployeeCommandValidator : AbstractValidator<DemoteGymStaffToPendingGymEmployeeCommand>
{
    public DemoteGymStaffToPendingGymEmployeeCommandValidator(ILocalizer localizer)
    {
        RuleFor(v => v.UserId)
            .PropertyOfEntityNotEmptyWithMessageLocalized(localizer, nameof(SharedResource.Id), nameof(SharedResource.User));
    }
}

public class DemoteGymStaffToPendingGymEmployeeCommandHandler : IRequestHandler<DemoteGymStaffToPendingGymEmployeeCommand, Result>
{
    private readonly IIdentityService _identityService;
    private readonly IUser _user;
    private readonly IApplicationDbContext _context;
    private readonly ILocalizer _localizer;

    public DemoteGymStaffToPendingGymEmployeeCommandHandler(
        IIdentityService identityService,
        IUser user,
        IApplicationDbContext context,
        ILocalizer localizer)
    {
        _identityService = identityService;
        _user = user;
        _context = context;
        _localizer = localizer;
    }
    public async Task<Result> Handle(DemoteGymStaffToPendingGymEmployeeCommand command, CancellationToken cancellationToken)
    {
        var demoterGymEmployment = await _context
            .GymEmployments
            .AsNoTracking()
            .FirstOrDefaultAsync(ge => ge.UserId == _user.Id, cancellationToken);

        Guard.Against.NullParameterRelatedToCurrentUser(demoterGymEmployment, nameof(GymEmployment), _user.Id);

        var gymStaffGymEmployment = await _context
            .GymEmployments
            .FirstOrDefaultAsync(ge => ge.UserId == command.UserId, cancellationToken);

        if (gymStaffGymEmployment is null)
        {
            return Result.NotFound(_localizer.GetNotFound(nameof(SharedResource.GymEmployment)));
        }

        if (gymStaffGymEmployment.GymId != demoterGymEmployment.GymId || gymStaffGymEmployment.Role != Roles.GymStaff)
        {
            return Result.Forbidden(nameof(SharedResource.Forbidden));
        }

        await using var transaction = await _context.BeginTransactionAsync(cancellationToken);

        try
        {
            var demotionResult = await _identityService.RemoveFromRoleAsync(command.UserId, Roles.GymStaff);

            if (!demotionResult.Succeeded)
            {
                await transaction.RollbackAsync(cancellationToken);

                throw new Exception($"Failed to remove user from their role. Result: {demotionResult}.");
            }

            var promotionResult = await _identityService.AddToRoleAsync(command.UserId, Roles.PendingGymEmployee);

            if (!promotionResult.Succeeded) //cannot be user not found if demotion succeeds
            {
                await transaction.RollbackAsync(cancellationToken);

                throw new Exception($"Failed to add user to role. Result: {promotionResult}.");
            }

            _context.GymEmployments.Remove(gymStaffGymEmployment);

            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return Result.Success();
        } catch
        {
            await transaction.RollbackAsync(cancellationToken);

            throw;
        }
    }
}
