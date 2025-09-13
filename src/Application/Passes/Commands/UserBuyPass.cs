using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Models;
using FitPass.Application.Common.Security;
using FitPass.Application.Extensions;
using FitPass.Domain;
using FitPass.Domain.Entities;

namespace FitPass.Application.Passes.Commands;

[Authorize]
public record UserBuyPassCommand(string GymPassProductId, string GymId) : IRequest<Result>;

public class UserBuyPassCommandValidator : AbstractValidator<UserBuyPassCommand>
{
    public UserBuyPassCommandValidator()
    {
        RuleFor(v => v.GymPassProductId).NotEmptyWithMessage("Gym pass product id");

        RuleFor(v => v.GymId).NotEmptyWithMessage("Gym id");
    }
}

public class UserBuyPassCommandHandler : IRequestHandler<UserBuyPassCommand, Result>
{
    private readonly IUser _user;
    private readonly IApplicationDbContext _context;
    public UserBuyPassCommandHandler(IUser user, IApplicationDbContext context)
    {
        _user = user;
        _context = context;
    }

    public async Task<Result> Handle(UserBuyPassCommand request, CancellationToken cancellationToken)
    {
        if (_user.Roles!.Count > 0)
        {
            return Result.Failure(["You are not allowed to buy passes on this account."]);
        }

        var gym = await _context.Gyms.Include(g => g.GymPassProducts).AsNoTracking().FirstOrDefaultAsync(g => g.Id == request.GymId, cancellationToken);

        if (gym == null)
        {
            return Result.Failure(["Gym with given id does not exist."]);
        }

        var gymPassProduct = gym.GymPassProducts.FirstOrDefault(gpp => gpp.Id == request.GymPassProductId);

        if (gymPassProduct == null)
        {
            return Result.Failure(["Gym pass product with given id does not exist."]);
        }

        var user = await _context.Users
                .Include(u => u.UserGymMemberships)
                .FirstOrDefaultAsync(u => u.Id == _user.Id, cancellationToken);

        if (user == null)
        {
            return Result.Failure(["Currently logged in user does not exist."]);
        }

        //since the user does not have a role they will have a collection of UserGymMemberships
        var userGymMembership = user.UserGymMemberships!.FirstOrDefault(ugm => ugm.GymId == request.GymId);


        if (userGymMembership == null)
        {
            userGymMembership = new UserGymMembership
            {
                Id = Guid.NewGuid().ToString(),
                ApplicationUserId = user.Id,
                GymId = request.GymId
            };

            user.UserGymMemberships!.Add(userGymMembership);
        }

        userGymMembership.OwnedPasses.Add(new OwnedPass
        {
            Id = Guid.NewGuid().ToString(),
            UserGymMembershipId = userGymMembership.Id,
            Type = gymPassProduct.Type,
            TotalUses = gymPassProduct.TotalUses,
            RemainingUses = gymPassProduct.TotalUses,
            ExpirationDate = gymPassProduct.ExpirationDate,
            EurPrice = gymPassProduct.EurPrice
        });

        var receipt = new PurchaseReceipt
        {
            Id = Guid.NewGuid().ToString(),
            ApplicationUserId = _user.Id,
            GymPassProduct = gymPassProduct
        };

        await _context.PurchaseReceipts.AddAsync(receipt);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
