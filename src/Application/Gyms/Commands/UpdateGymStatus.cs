using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Security;
using FitPass.Application.Extensions;
using FitPass.Domain.Constants;
using FitPass.Domain.Enums;

namespace Fitpass.Application.Gyms.Commands;

[Authorize(Roles = Roles.AppAdministrator)]
public record UpdateGymStatusCommand(string GymId, GymStatus NewGymStatus) : IRequest;

public class UpdateGymStatusCommandValidator : AbstractValidator<UpdateGymStatusCommand>
{
    public UpdateGymStatusCommandValidator()
    {
        RuleFor(v => v.GymId).NotEmptyWithMessage("Gym id");
    }
}

public class UpdateGymStatusCommandHandler : IRequestHandler<UpdateGymStatusCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateGymStatusCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(UpdateGymStatusCommand command, CancellationToken cancellationToken)
    {
        var gym = await _context.Gyms.FindAsync(command.GymId, cancellationToken);

        Guard.Against.NotFound(command.GymId, gym, "Id");

        gym.Status = command.NewGymStatus;

        await _context.SaveChangesAsync(cancellationToken);
    }
}