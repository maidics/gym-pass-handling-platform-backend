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
            .NotEmptyWithMessage("New gym status");
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

        Guard.Against.NullEntityRelatedToCurrentUser(gymEmployment, nameof(GymEmployment), _user.Id);

        var gym = await _context
            .Gyms
            .FindAsync(gymEmployment.GymId, cancellationToken);

        Guard.Against.NullEntityRelatedToCurrentUser(gym, "gym employee managed gym", _user.Id);

        if (gym.Status == command.NewGymStatus)
        {
            return Result.Success();
        }

        gym.Status = command.NewGymStatus;

        await _context.SaveChangesAsync();

        return Result.Success();
    }
}
