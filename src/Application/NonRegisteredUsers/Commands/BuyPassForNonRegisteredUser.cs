using FitPass.Application.Common.Interfaces;
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
) : IRequest;

public class BuyPassForNonRegisteredUserCommandValidator : AbstractValidator<BuyPassForNonRegisteredUserCommand>
{
    public BuyPassForNonRegisteredUserCommandValidator()
    {
        RuleFor(v => v.NonRegisteredUserId).NotEmptyWithMessage("Non registered user id");

        RuleFor(v => v.GymPassProductId).NotEmptyWithMessage("Gym pass product id");
    }
}

public class BuyPassForNonRegisteredUserCommandHandler : IRequestHandler<BuyPassForNonRegisteredUserCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public BuyPassForNonRegisteredUserCommandHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }
    public async Task Handle(BuyPassForNonRegisteredUserCommand command, CancellationToken cancellationToken)
    {
        var nonRegisteredUser = await _context
            .NonRegisteredUsers
            .Include(nru => nru.UserGymMemberships)
            .FirstOrDefaultAsync(nru => nru.Id == command.NonRegisteredUserId);

        Guard.Against.NotFound(command.NonRegisteredUserId, nonRegisteredUser, "Id");

        var gymPassProduct = await _context
            .GymPassProducts
            .AsNoTracking()
            .FirstOrDefaultAsync(gpp => gpp.Id == command.GymPassProductId, cancellationToken);

        Guard.Against.NotFound(command.GymPassProductId, gymPassProduct, "Id");

        var gymStaffAssignment = await _context.GymStaffAssigments
            .AsNoTracking()
            .FirstOrDefaultAsync(gsa => gsa.ApplicationUserId == _user.Id, cancellationToken);

        Guard.Against.Null(gymStaffAssignment, "Id", "Failed to find currently logged in Gym Admin or Gym Staff member");

        var userGymMembership = nonRegisteredUser.UserGymMemberships.FirstOrDefault(ugm => ugm.GymId == gymStaffAssignment!.GymId);

        if (userGymMembership == null)
        {
            userGymMembership = new UserGymMembership
            {
                Id = Guid.NewGuid().ToString(),
                UserId = nonRegisteredUser.Id,
                GymId = gymStaffAssignment!.GymId!
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
            EurPrice = gymPassProduct.HUFPrice
        };

        userGymMembership.OwnedPasses.Add(ownedPass);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
