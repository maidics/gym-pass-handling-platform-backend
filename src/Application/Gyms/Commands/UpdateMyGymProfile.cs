using FitPass.Application.Common.Extensions;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Security;
using FitPass.Domain.Constants;
using FitPass.Domain.Enums;
using FitPass.Domain.Entities;
using FitPass.Application.Common.Models;
using FitPass.Domain.ValueObjects;
using FitPass.Application.Common.Resources;

namespace FitPass.Application.Gyms.Commands;

[Authorize(Roles = Roles.GymAdministrator)]
public record UpdateMyGymProfileCommand(
    string NewName,
    Address NewAddress,
    GymTier NewTier
) : IRequest<Result>;

public class UpdateMyGymProfileCommandValidator : AbstractValidator<UpdateMyGymProfileCommand>
{
    public UpdateMyGymProfileCommandValidator(ILocalizer localizer)
    {
        RuleFor(v => v.NewName)
            .NotEmpty()
            .WithMessage(localizer.GetNewValueIsRequired(nameof(SharedResource.Name)));
    }
}

public class UpdateMyGymProfileCommandHandler : IRequestHandler<UpdateMyGymProfileCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly ILocalizer _localizer;

    public UpdateMyGymProfileCommandHandler(
        IApplicationDbContext context,
        IUser user,
        ILocalizer localizer)
    {
        _context = context;
        _user = user;
        _localizer = localizer;
    }
    public async Task<Result> Handle(UpdateMyGymProfileCommand command, CancellationToken cancellationToken)
    {
        var gymEmployment = await _context
            .GymEmployments
            .AsNoTracking()
            .FirstOrDefaultAsync(ge => ge.UserId == _user.Id);

        Guard.Against.NullParameterRelatedToCurrentUser(gymEmployment, nameof(GymEmployment), _user.Id);

        var gym = await _context.Gyms.FindAsync(gymEmployment.GymId, cancellationToken);

        Guard.Against.Null(gym, nameof(Gym), "Gym not found.");

        if (await _context.Gyms.AnyAsync(g => g.Id != gym.Id && g.Name == command.NewName, cancellationToken))
        {
            return Result.Conflict(
                _localizer.Get(nameof(SharedResource.Conflict), 
                    _localizer.GetWithParamsLocalized(nameof(SharedResource.NewValue), nameof(SharedResource.Name))));
        }

        gym.Name = command.NewName;
        gym.Address = command.NewAddress;
        gym.Tier = command.NewTier;

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
