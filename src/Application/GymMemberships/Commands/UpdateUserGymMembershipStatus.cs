using FitPass.Application.Common.Exceptions;
using FitPass.Application.Common.Extensions;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Logging;
using FitPass.Application.Common.Security;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using FitPass.Domain.Enums;
using FitPass.Domain.Strings;
using Microsoft.Extensions.Logging;

namespace FitPass.Application.GymMemberships.Commands;

[Authorize(Roles = $"{Roles.GymAdministrator},{Roles.GymStaff}")]
public record UpdateGymMembershipStatusCommand(string GymMembershipId, GymMembershipStatus NewStatus) : IRequest;

public class UpdateGymMembershipStatusCommandValidator : AbstractValidator<UpdateGymMembershipStatusCommand>
{
    public UpdateGymMembershipStatusCommandValidator()
    {
        RuleFor(v => v.GymMembershipId).NotEmptyWithMessage(nameof(UpdateGymMembershipStatusCommand.GymMembershipId));
    }
}

public class UpdateGymMembershipStatusCommandHandler : IRequestHandler<UpdateGymMembershipStatusCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly ILogger<UpdateGymMembershipStatusCommand> _logger;

    public UpdateGymMembershipStatusCommandHandler(
        IApplicationDbContext context,
        IUser user,
        ILogger<UpdateGymMembershipStatusCommand> logger)
    {
        _context = context;
        _user = user;
        _logger = logger;
    }
    public async Task Handle(UpdateGymMembershipStatusCommand command, CancellationToken cancellationToken)
    {
        var gymEmployment = await _context.GymEmployments
            .AsNoTracking()
            .FirstOrDefaultAsync(gsa => gsa.UserId == _user.Id, cancellationToken);

        if (gymEmployment is null)
        {
            LogCriticalMessages.AuthenticatedUserRelatedEntityNotFound(
                _logger,
                _user.Roles,
                _user.Id,
                nameof(GymEmployment));

            throw new Exception(ErrorMessages.AuthenticatedUserRelatedEntityNotFound(nameof(GymEmployment)));
        }

        var userGymMembership = await _context.GymMemberships.FindAsync(command.GymMembershipId, cancellationToken);

        Guard.Against.NotFound(command.GymMembershipId, userGymMembership, "Id");

        if (userGymMembership.GymId != gymEmployment.GymId)
        {
            throw new ForbiddenAccessException();
        }

        userGymMembership.Status = command.NewStatus;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
