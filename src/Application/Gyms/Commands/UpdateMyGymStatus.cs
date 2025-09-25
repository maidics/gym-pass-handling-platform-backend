using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Security;
using FitPass.Application.Extensions;
using FitPass.Domain.Constants;
using FitPass.Domain.Enums;

namespace FitPass.Application.Gyms.Commands;

[Authorize(Roles = Roles.GymAdministrator)]
public record UpdateMyGymStatusCommand(GymStatus NewGymStatus) : IRequest;

public class UpdateMyGymStatusCommandValidator : AbstractValidator<UpdateMyGymStatusCommand>
{
    public UpdateMyGymStatusCommandValidator()
    {
        RuleFor(v => v.NewGymStatus).NotEmptyWithMessage("New gym status");
    }
}

public class UpdateMyGymStatusCommandHandler : IRequestHandler<UpdateMyGymStatusCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IUserProfileService _userProfileService;
    private readonly IUser _user;

    public UpdateMyGymStatusCommandHandler(IApplicationDbContext context, IUserProfileService userProfileService, IUser user)
    {
        _context = context;
        _userProfileService = userProfileService;
        _user = user;
    }
    public async Task Handle(UpdateMyGymStatusCommand command, CancellationToken cancellationToken)
    {
        var gymStaffAssignment = await _userProfileService.GetUserGymStaffAssigmentAsync(_user.Id!, cancellationToken);

        Guard.Against.Null(gymStaffAssignment, "Id", "Failed to find currently logged in Gym Admin.");

        var gym = await _context
            .Gyms
            .FindAsync(gymStaffAssignment.GymId, cancellationToken);

        Guard.Against.Null(gym, "Id", "Failed to find Gym Admin's managed gym.");

        if (gym.Status == command.NewGymStatus)
        {
            return;
        }

        gym.Status = command.NewGymStatus;

        await _context.SaveChangesAsync();
    }
}