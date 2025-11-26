using FitPass.Application.Common.Extensions;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Models;
using FitPass.Application.Common.Security;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using FitPass.Domain.Enums;

namespace FitPass.Application.Gyms.Commands;

[Authorize(Roles = Roles.AppAdministrator)]
public record UpdateGymStatusCommand(string GymId, GymStatus NewGymStatus) : IRequest<Result>;

public class UpdateGymStatusCommandValidator : AbstractValidator<UpdateGymStatusCommand>
{
    public UpdateGymStatusCommandValidator()
    {
        RuleFor(v => v.GymId).NotEmptyWithMessage(nameof(UpdateGymStatusCommand.GymId));

        RuleFor(v => v.NewGymStatus)
            .NotEmptyWithMessage(nameof(UpdateGymStatusCommand.NewGymStatus));
    }
}

public class UpdateGymStatusCommandHandler : IRequestHandler<UpdateGymStatusCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public UpdateGymStatusCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(UpdateGymStatusCommand command, CancellationToken cancellationToken)
    {
        var gym = await _context.Gyms.FindAsync(command.GymId, cancellationToken);

        if (gym is null)
        {
            return Result.NotFound(nameof(Gym));
        }

        gym.Status = command.NewGymStatus;

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
