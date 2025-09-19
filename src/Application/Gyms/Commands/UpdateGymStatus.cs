using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Models;
using FitPass.Application.Common.Security;
using FitPass.Application.Extensions;
using FitPass.Domain.Constants;
using FitPass.Domain.Enums;

namespace Fitpass.Application.Gyms.Commands;

[Authorize(Roles = $"{Roles.AppAdministrator},{Roles.GymAdministrator}")]
public record UpdateGymStatusCommand(string GymID, GymStatus NewGymStatus) : IRequest<Result>;

public class UpdateGymStatusCommandValidator : AbstractValidator<UpdateGymStatusCommand>
{
    public UpdateGymStatusCommandValidator()
    {
        RuleFor(v => v.GymID).NotEmptyWithMessage("Gym id");
    }
}

public class UpdateGymStatusCommandHandler : IRequestHandler<UpdateGymStatusCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _identityService;
    private readonly IUser _user;

    public UpdateGymStatusCommandHandler(IApplicationDbContext context, IIdentityService identityService, IUser user)
    {
        _context = context;
        _identityService = identityService;
        _user = user;
    }

    public async Task<Result> Handle(UpdateGymStatusCommand command, CancellationToken cancellationToken)
    {
        var gym = await _context.Gyms.FindAsync(command.GymID);

        if (gym == null)
        {
            return Result.Failure(["Gym not found"]);
        }

        if (_user.Roles!.Contains(Roles.AppAdministrator))
        {
            gym.Status = command.NewGymStatus;

            await _context.SaveChangesAsync();

            return Result.Success();
        }

        var gymStaffAssigment = await _context
            .GymStaffAssigments
            .AsNoTracking()
            .FirstOrDefaultAsync(gsa => gsa.ApplicationUserId == _user.Id);

        if (gymStaffAssigment!.GymId != command.GymID)
        {
            return Result.Failure(["You are not allowed to change the status of this gym."]);
        }

        if (_user.Roles.Contains(Roles.GymAdministrator) && (command.NewGymStatus == GymStatus.Active || command.NewGymStatus == GymStatus.Inactive))
        {
            gym.Status = command.NewGymStatus;

            await _context.SaveChangesAsync();

            return Result.Success();
        }

        return Result.Failure(["You are not allowed to change the status of this gym"]);
    }
}