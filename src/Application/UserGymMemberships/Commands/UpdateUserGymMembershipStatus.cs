using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Security;
using FitPass.Application.Extensions;
using FitPass.Domain.Constants;
using FitPass.Domain.Enums;

namespace Fitpass.Application.UserGymMemberships.Commands;

[Authorize(Roles = $"{Roles.GymAdministrator},{Roles.GymStaff}")]
public record UpdateUserGymMembershipStatusCommand(string UserGymMembershipId, GymMembershipStatus NewStatus) : IRequest;

public class UpdateUserGymMembershipStatusCommandValidator : AbstractValidator<UpdateUserGymMembershipStatusCommand>
{
    public UpdateUserGymMembershipStatusCommandValidator()
    {
        RuleFor(v => v.UserGymMembershipId).NotEmptyWithMessage("User gym membership id");

        RuleFor(v => v.NewStatus).NotEmptyWithMessage("New gym membership status");
    }
}

public class UpdateUserGymMembershipStatusCommandHandler : IRequestHandler<UpdateUserGymMembershipStatusCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IUserProfileService _userProfileService;
    private readonly IUser _user;

    public UpdateUserGymMembershipStatusCommandHandler(IApplicationDbContext context, IUserProfileService userProfileService, IUser user)
    {
        _context = context;
        _userProfileService = userProfileService;
        _user = user;
    }
    public async Task Handle(UpdateUserGymMembershipStatusCommand command, CancellationToken cancellationToken)
    {
        var gymStaffAssigment = await _userProfileService.GetUserGymStaffAssigmentAsync(_user.Id!, cancellationToken);

        Guard.Against.Null(gymStaffAssigment, "Id", "Failed to find currently logged in Gym Admin or Gym Staff member.");

        var userGymMembership = await _context.UserGymMemberships.FindAsync(command.UserGymMembershipId, cancellationToken);

        Guard.Against.NotFound(command.UserGymMembershipId, userGymMembership, "Id");

        if (userGymMembership.GymId != gymStaffAssigment.GymId)
        {
            throw new UnauthorizedAccessException();
        }

        userGymMembership.GymMembershipStatus = command.NewStatus;

        await _context.SaveChangesAsync(cancellationToken);
    }
}