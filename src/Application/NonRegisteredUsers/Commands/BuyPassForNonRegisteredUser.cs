using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Models;
using FitPass.Application.Common.Security;
using FitPass.Application.Extensions;
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

        var gymStaffAssignment = await _userProfileService.GetUserGymStaffAssigment(_user.Id!, cancellationToken);

        var userGymMembership = nonRegisteredUser.UserGymMemberships.FirstOrDefault(ugm => ugm.GymId == gymStaffAssignment!.GymId);

        if (userGymMembership == null)
        {
            userGymMembership = new UserGymMembership
            {
                Id = Guid.NewGuid().ToString(),
                UserId = nonRegisteredUser.Id,
                GymId = gymStaffAssignment!.GymId
            };

            nonRegisteredUser
        }
    }
}