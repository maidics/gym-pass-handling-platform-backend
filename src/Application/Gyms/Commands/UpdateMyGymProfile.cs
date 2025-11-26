using FitPass.Application.Common.Extensions;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Security;
using FitPass.Domain.Constants;
using FitPass.Domain.Enums;
using FitPass.Domain.Entities;
using FitPass.Application.Common.Models;

namespace FitPass.Application.Gyms.Commands;

[Authorize(Roles = Roles.GymAdministrator)]
public record UpdateMyGymProfileCommand(
    string GymName,
    string GymAddress,
    GymTier GymTier,
    string? GymOwnerName
) : IRequest<Result>;

public class UpdateMyGymProfileCommandValidator : AbstractValidator<UpdateMyGymProfileCommand>
{
    public UpdateMyGymProfileCommandValidator()
    {
        RuleFor(v => v.GymName).NotEmptyWithMaxLenghtAndMessage(nameof(UpdateMyGymProfileCommand.GymName), MaxStringLengths.Name);

        RuleFor(v => v.GymAddress).NotEmptyWithMaxLenghtAndMessage(nameof(UpdateMyGymProfileCommand.GymAddress), MaxStringLengths.Address);
    }
}

public class UpdateMyGymProfileCommandHandler : IRequestHandler<UpdateMyGymProfileCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public UpdateMyGymProfileCommandHandler(
        IApplicationDbContext context,
        IUser user)
    {
        _context = context;
        _user = user;
    }
    public async Task<Result> Handle(UpdateMyGymProfileCommand command, CancellationToken cancellationToken)
    {
        var gymEmployment = await _context
            .GymEmployments
            .AsNoTracking()
            .FirstOrDefaultAsync(ge => ge.UserId == _user.Id);

        Guard.Against.NullEntityRelatedToCurrentUser(gymEmployment, nameof(GymEmployment), _user.Id);

        var gym = await _context.Gyms.FindAsync(gymEmployment.GymId, cancellationToken);

        if (gym is null)
        {
            return Result.NotFound(nameof(Gym));
        }

        gym.Name = command.GymName;
        gym.Address = command.GymAddress;
        gym.Tier = command.GymTier;

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
