using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Security;
using FitPass.Application.Extensions;
using FitPass.Domain;
using FitPass.Domain.Entities;

namespace FitPass.Application.Passes.Commands;

[Authorize]
public record ApplicationUserBuyPassCommand(string GymPassProductId) : IRequest;

public class ApplicationUserBuyPassCommandValidator : AbstractValidator<ApplicationUserBuyPassCommand>
{
    public ApplicationUserBuyPassCommandValidator()
    {
        RuleFor(v => v.GymPassProductId).NotEmptyWithMessage(nameof(ApplicationUserBuyPassCommand.GymPassProductId));
    }
}

public class ApplicationUserBuyPassCommandHandler : IRequestHandler<ApplicationUserBuyPassCommand>
{
    private readonly IUser _user;
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _identityService;
    public ApplicationUserBuyPassCommandHandler(IUser user, IApplicationDbContext context, IIdentityService identityService)
    {
        _user = user;
        _context = context;
        _identityService = identityService;
    }

    public async Task Handle(ApplicationUserBuyPassCommand command, CancellationToken cancellationToken)
    {
        cancellationToken = CancellationToken.None;

        if (_user.Roles!.Count > 0)
        {
            throw new UnauthorizedAccessException("You are not allowed to buy passes on this account.");
        }

        var gymPassProduct = await _context
            .GymPassProducts
            .AsNoTracking()
            .Include(gpp => gpp.Gym)
            .FirstOrDefaultAsync(gpp => gpp.Id == command.GymPassProductId);

        Guard.Against.NotFound(command.GymPassProductId, gymPassProduct, "Id");

        var user = await _identityService.FindUserByIdAsync(_user.Id!);

        Guard.Against.Null(user, "Id", "Failed to find currently logged in user.");

        var userGymMembership = user.UserGymMemberships!.FirstOrDefault(ugm => ugm.GymId == gymPassProduct.GymId);

        if (userGymMembership == null)
        {
            userGymMembership = new GymMembership
            {
                ApplicationUserId = user.Id,
                NonRegisteredUserId = null,
                GymId = gymPassProduct.GymId
            };

            user.UserGymMemberships!.Add(userGymMembership);
        }

        userGymMembership.OwnedPasses.Add(new GymMembershipPass
        {
            GymMembershipId = userGymMembership.Id,
            Type = gymPassProduct.Type,
            TotalUses = gymPassProduct.TotalUses,
            RemainingUses = gymPassProduct.TotalUses,
            ExpirationDate = gymPassProduct.GetExpirationDate(),
            EurPrice = gymPassProduct.HUFPrice
        });

        var receipt = new PurchaseReceipt
        {
            UserPaymentProfileId = userGymMembership.Id,
            GymPassProduct = gymPassProduct
        };

        await _context.PurchaseReceipts.AddAsync(receipt);
        await _context.SaveChangesAsync();
    }
}
