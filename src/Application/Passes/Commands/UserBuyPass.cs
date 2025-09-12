using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Models;
using FitPass.Application.Common.Security;
using FitPass.Application.Extensions;
using FitPass.Domain.Entities;

namespace FitPass.Application.Passes.Commands;

[Authorize]
public record UserBuyPassCommand(string GymPassProductId) : IRequest<Result>;

public class UserBuyPassCommandValidator : AbstractValidator<UserBuyPassCommand>
{
    public UserBuyPassCommandValidator()
    {
        RuleFor(v => v.GymPassProductId).NotEmptyWithMessage("Gym pass product id");
    }
}

public class UserBuyPassCommandHandler : IRequestHandler<UserBuyPassCommand, Result>
{
    private readonly IUser _user;
    private readonly IApplicationDbContext _context;
    private readonly IUserProfileService _userProfileService;

    public UserBuyPassCommandHandler(IUser user, IApplicationDbContext context, IUserProfileService userProfileService)
    {
        _user = user;
        _context = context;
        _userProfileService = userProfileService;
    }

    public async Task<Result> Handle(UserBuyPassCommand request, CancellationToken cancellationToken)
    {
        if (_user.Roles!.Count > 0)
        {
            return Result.Failure(["You are not allowed to buy passes on this account."]);
        }

        var gymPassProduct = await _context.GymPassProducts.AsNoTracking().FirstOrDefaultAsync(gpp => gpp.Id == request.GymPassProductId);

        if (gymPassProduct == null)
        {
            return Result.Failure(["Gym pass product not found."]);
        }

        var user = await _userProfileService.GetUserGymMembershipsAsync(_user.Id!, cancellationToken);

        var receipt = new PurchaseReceipt
        {
            Id = Guid.NewGuid().ToString(),
            ApplicationUserId = _user.Id,
            GymPassProduct = gymPassProduct
        };
    }
}