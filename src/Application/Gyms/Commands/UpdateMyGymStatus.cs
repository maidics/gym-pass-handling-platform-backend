using FitPass.Application.Common.Extensions;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Logging;
using FitPass.Application.Common.Security;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using FitPass.Domain.Enums;
using FitPass.Domain.Strings;
using Microsoft.Extensions.Logging;

namespace FitPass.Application.Gyms.Commands;

[Authorize(Roles = Roles.GymAdministrator)]
public record UpdateMyGymStatusCommand(GymStatus NewGymStatus) : IRequest;

public class UpdateMyGymStatusCommandValidator : AbstractValidator<UpdateMyGymStatusCommand>
{
    public UpdateMyGymStatusCommandValidator()
    {
        RuleFor(v => v.NewGymStatus)
            .NotEmptyWithMessage("New gym status");
    }
}

public class UpdateMyGymStatusCommandHandler : IRequestHandler<UpdateMyGymStatusCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly ILogger<UpdateMyGymStatusCommandHandler> _logger;

    public UpdateMyGymStatusCommandHandler(IApplicationDbContext context, IUser user, ILogger<UpdateMyGymStatusCommandHandler> logger)
    {
        _context = context;
        _user = user;
        _logger = logger;
    }
    public async Task Handle(UpdateMyGymStatusCommand command, CancellationToken cancellationToken)
    {
        var gymEmployment = await _context.GymEmployments
            .AsNoTracking()
            .FirstOrDefaultAsync(gsa => gsa.UserId == _user.Id, cancellationToken);

        if (gymEmployment == null)
        {
            LogCriticalMessages.AuthenticatedUserRelatedEntityNotFound(_logger, _user.Roles, _user.Id, nameof(GymEmployment));

            throw new Exception(ErrorMessages.AuthenticatedUserRelatedEntityNotFound(nameof(GymEmployment)));
        }

        var gym = await _context
            .Gyms
            .FindAsync(gymEmployment.GymId, cancellationToken);

        Guard.Against.Null(gym, "Id", "Failed to find Gym Admin's managed gym.");

        if (gym.Status == command.NewGymStatus)
        {
            return;
        }

        gym.Status = command.NewGymStatus;

        await _context.SaveChangesAsync();
    }
}
