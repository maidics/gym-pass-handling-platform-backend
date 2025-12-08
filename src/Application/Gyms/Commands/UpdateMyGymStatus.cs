using FitPass.Application.Common.Extensions;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Models;
using FitPass.Application.Common.Security;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using FitPass.Domain.Enums;

namespace FitPass.Application.Gyms.Commands;

[Authorize(Roles = Roles.GymAdministrator)]
public record UpdateMyGymStatusCommand(GymStatus NewGymStatus) : IRequest<Result>;

public class UpdateMyGymStatusCommandValidator : AbstractValidator<UpdateMyGymStatusCommand>
{
    public UpdateMyGymStatusCommandValidator()
    {
        RuleFor(v => v.NewGymStatus)
            .Must(status => status == GymStatus.Active || status == GymStatus.Inactive)
            .WithMessage("You can only set your gym status to active or inactive.");
    }
}

public class UpdateMyGymStatusCommandHandler : IRequestHandler<UpdateMyGymStatusCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public UpdateMyGymStatusCommandHandler(
        IApplicationDbContext context, 
        IUser user)
    {
        _context = context;
        _user = user;
    }
    public async Task<Result> Handle(UpdateMyGymStatusCommand command, CancellationToken cancellationToken)
    {
        var gymEmployment = await _context.GymEmployments
            .AsNoTracking()
            .FirstOrDefaultAsync(gsa => gsa.UserId == _user.Id, cancellationToken);

        Guard.Against.NullParameterRelatedToCurrentUser(gymEmployment, nameof(GymEmployment), _user.Id);

        var gym = await _context
            .Gyms
            .FindAsync(gymEmployment.GymId, cancellationToken);

        Guard.Against.Null(gym, nameof(Gym), "Gym not found.");

        if (gym.Status == GymStatus.Suspended)
        {
            return Result.BusinessRuleViolation("Cannot change status of a suspended gym.");
        }

        if (gym.Status == command.NewGymStatus)
        {
            return Result.Success();
        }

        gym.Status = command.NewGymStatus;

        await _context.SaveChangesAsync();

        return Result.Success();
    }
}
