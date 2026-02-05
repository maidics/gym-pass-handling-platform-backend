using FitPass.Application.Common.Extensions;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Models;
using FitPass.Application.Common.Resources;
using FitPass.Application.Common.Security;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;

namespace FitPass.Application.Users.Commands.RoleHandling;

[Authorize(Roles = Roles.GymAdministrator)]
public record PromotePendingGymEmployeeToGymStaffRoleCommand(string PendingGymEmployeeEmail)
    : IRequest<Result>;

public class PromotePendingGymEmployeeToGymStaffRoleCommandValidator
    : AbstractValidator<PromotePendingGymEmployeeToGymStaffRoleCommand>
{
    public PromotePendingGymEmployeeToGymStaffRoleCommandValidator(ILocalizer localizer)
    {
        RuleFor(v => v.PendingGymEmployeeEmail).EmailAddressWithMessageLocalized(localizer);
    }
}

public class PromotePendingGymEmployeeToGymStaffRoleCommandHandler
    : IRequestHandler<PromotePendingGymEmployeeToGymStaffRoleCommand, Result>
{
    private readonly IIdentityService _identityService;
    private readonly IUser _user;
    private readonly IApplicationDbContext _context;
    private readonly ILocalizer _localizer;

    public PromotePendingGymEmployeeToGymStaffRoleCommandHandler(
        IIdentityService identityService,
        IUser user,
        IApplicationDbContext context,
        ILocalizer localizer
    )
    {
        _identityService = identityService;
        _user = user;
        _context = context;
        _localizer = localizer;
    }

    public async Task<Result> Handle(
        PromotePendingGymEmployeeToGymStaffRoleCommand command,
        CancellationToken cancellationToken
    )
    {
        var promoterGymEmployment = await _context
            .GymEmployments.AsNoTracking()
            .FirstOrDefaultAsync(ge => ge.UserId == _user.Id, cancellationToken);

        Guard.Against.NullParameterRelatedToCurrentUser(
            promoterGymEmployment,
            "Promoter GymEmployment",
            _user.Id
        );

        var userId = await _identityService.GetUserIdByEmailAsync(command.PendingGymEmployeeEmail);

        if (
            userId is null
            || !await _identityService.IsInRoleAsync(userId, Roles.PendingGymEmployee)
        )
        {
            return Result.NotFound(_localizer.GetNotFound(nameof(SharedResource.User)));
        }

        await using var transaction = await _context.BeginTransactionAsync(cancellationToken);

        try
        {
            var demotionResult = await _identityService.RemoveFromRoleAsync(
                userId,
                Roles.PendingGymEmployee
            );

            if (!demotionResult.Succeeded)
            {
                await transaction.RollbackAsync(cancellationToken);

                throw new Exception(
                    $"Failed to remove user from their role. Result: {demotionResult}."
                );
            }

            var promotionResult = await _identityService.AddToRoleAsync(userId, Roles.GymStaff);

            if (!promotionResult.Succeeded) //cannot be user not found if demotion succeeds
            {
                await transaction.RollbackAsync(cancellationToken);

                throw new Exception($"Failed to add user to role. Result: {promotionResult}.");
            }

            var gymEmployment = new GymEmployment
            {
                UserId = userId,
                GymId = promoterGymEmployment.GymId,
                Role = Roles.GymStaff,
            };

            await _context.GymEmployments.AddAsync(gymEmployment, cancellationToken);

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
