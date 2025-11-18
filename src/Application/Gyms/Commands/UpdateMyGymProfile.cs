using FitPass.Application.Common.Extensions;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Security;
using FitPass.Domain.Constants;
using FitPass.Domain.Enums;
using FitPass.Domain.Entities;
using FitPass.Application.Common.Logging;
using Microsoft.Extensions.Logging;
using FitPass.Domain.Strings;

namespace FitPass.Application.Gyms.Commands;

[Authorize(Roles = Roles.GymAdministrator)]
public record UpdateMyGymProfileCommand(
    string GymName,
    string GymAddress,
    GymTier GymTier,
    string? GymOwnerName
) : IRequest;

public class UpdateMyGymProfileCommandValidator : AbstractValidator<UpdateMyGymProfileCommand>
{
    public UpdateMyGymProfileCommandValidator()
    {
        RuleFor(v => v.GymName).NotEmptyWithMaxLenghtAndMessage(nameof(UpdateMyGymProfileCommand.GymName), MaxStringLengths.Name);

        RuleFor(v => v.GymAddress).NotEmptyWithMaxLenghtAndMessage(nameof(UpdateMyGymProfileCommand.GymAddress), MaxStringLengths.Address);
    }
}

public class UpdateMyGymProfileCommandHandler : IRequestHandler<UpdateMyGymProfileCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly ILogger<UpdateMyGymProfileCommand> _logger;

    public UpdateMyGymProfileCommandHandler(
        IApplicationDbContext context,
        IUser user,
        ILogger<UpdateMyGymProfileCommand> logger)
    {
        _context = context;
        _user = user;
        _logger = logger;
    }
    public async Task Handle(UpdateMyGymProfileCommand command, CancellationToken cancellationToken)
    {
        var gymEmployment = await _context
            .GymEmployments
            .AsNoTracking()
            .FirstOrDefaultAsync(ge => ge.ApplicationUserId == _user.Id);

        if (gymEmployment is null)
        {
            LogCriticalMessages.AuthenticatedUserRelatedEntityNotFound(
                _logger,
                _user.Roles,
                _user.Id,
                nameof(GymEmployment));

            throw new SystemException(ErrorMessages.AuthenticatedUserRelatedEntityNotFound(nameof(GymEmployment)));
        }

        var gym = await _context.Gyms.FindAsync(gymEmployment.GymId, cancellationToken);

        Guard.Against.NotFound(gymEmployment.GymId!, gym, "Gym");

        gym.Name = command.GymName;
        gym.Address = command.GymAddress;
        gym.Tier = command.GymTier;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
