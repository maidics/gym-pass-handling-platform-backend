using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Models;
using FitPass.Application.Common.Security;
using FitPass.Application.Extensions;
using FitPass.Domain;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;

namespace Fitpass.Application.NonRegisteredUsers.Commands;

[Authorize(Roles = $"{Roles.GymAdministrator},{Roles.GymStaff}")]
public record BuyPassForNonRegisteredUserCommand(
    string NonRegisteredUserId,
    string GymPassProductId
) : IRequest<Result>;

public class BuyPassForNonRegisteredUserCommandValidator : AbstractValidator<BuyPassForNonRegisteredUserCommand>
{
    public BuyPassForNonRegisteredUserCommandValidator()
    {
        RuleFor(v => v.NonRegisteredUserId).NotEmptyWithMessage("Non registered user id");

        RuleFor(v => v.GymPassProductId).NotEmptyWithMessage("Gym pass product id");
    }
}

public class BuyPassForNonRegisteredUserCommandHandler : IRequestHandler<BuyPassForNonRegisteredUserCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly IUserProfileService _userProfileService;

    public BuyPassForNonRegisteredUserCommandHandler(IApplicationDbContext context, IUser user, IUserProfileService userProfileService)
    {
        _context = context;
        _user = user;
        _userProfileService = userProfileService;
    }
    public async Task<Result> Handle(BuyPassForNonRegisteredUserCommand command, CancellationToken cancellationToken)
    {
        var nonRegisteredUser = await _context
            .NonRegisteredUsers
            .Include(nru => nru.UserGymMemberships)
            .FirstOrDefaultAsync(nru => nru.Id == command.NonRegisteredUserId);

        if (nonRegisteredUser == null)
        {
            return Result.Failure(["Non registered user not found."]);
        }

        var gymPassProduct = await _context.GymPassProducts.AsNoTracking().FirstOrDefaultAsync(gpp => gpp.Id == command.GymPassProductId, cancellationToken);

        if (gymPassProduct == null)
        {
            return Result.Failure(["Gym pass product not found."]);
        }

        var gymStaffAssignment = await _userProfileService.GetUserGymStaffAssigmentAsync(_user.Id!, cancellationToken);

        var userGymMembership = nonRegisteredUser.UserGymMemberships.FirstOrDefault(ugm => ugm.GymId == gymStaffAssignment!.GymId);

        if (userGymMembership == null)
        {
            userGymMembership = new UserGymMembership
            {
                Id = Guid.NewGuid().ToString(),
                UserId = nonRegisteredUser.Id,
                GymId = gymStaffAssignment!.GymId
            };

            nonRegisteredUser.UserGymMemberships.Add(userGymMembership);
        }

        var utcNow = DateTimeOffset.UtcNow;

        var ownedPass = new OwnedPass
        {
            Id = Guid.NewGuid().ToString(),
            UserGymMembershipId = userGymMembership.Id,
            Type = gymPassProduct.Type,
            TotalUses = gymPassProduct.TotalUses,
            RemainingUses = gymPassProduct.TotalUses,
            ExpirationDate = gymPassProduct.GetExpirationDate(),
            EurPrice = gymPassProduct.EurPrice
        };

        userGymMembership.OwnedPasses.Add(ownedPass);

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
