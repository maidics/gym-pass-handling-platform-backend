using FitPass.Application.Common.Extensions;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Models;
using FitPass.Application.Common.Security;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using FitPass.Domain.Enums;

namespace FitPass.Application.GymMemberships.Commands;

[Authorize(Roles = $"{Roles.GymAdministrator},{Roles.GymStaff}")]
public record UpdateGymMembershipStatusCommand(string GymMembershipId, GymMembershipStatus NewStatus) : IRequest<Result>;

public class UpdateGymMembershipStatusCommandValidator : AbstractValidator<UpdateGymMembershipStatusCommand>
{
    public UpdateGymMembershipStatusCommandValidator()
    {
        RuleFor(v => v.GymMembershipId).NotEmptyWithMessage(nameof(UpdateGymMembershipStatusCommand.GymMembershipId));
    }
}

public class UpdateGymMembershipStatusCommandHandler : IRequestHandler<UpdateGymMembershipStatusCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public UpdateGymMembershipStatusCommandHandler(
        IApplicationDbContext context,
        IUser user)
    {
        _context = context;
        _user = user;
    }
    public async Task<Result> Handle(UpdateGymMembershipStatusCommand command, CancellationToken cancellationToken)
    {
        var gymEmployment = await _context.GymEmployments
            .AsNoTracking()
            .FirstOrDefaultAsync(gsa => gsa.UserId == _user.Id, cancellationToken);

        Guard.Against.NullEntityRelatedToCurrentUser(gymEmployment, nameof(GymEmployment), _user.Id);

        var membership = await _context.GymMemberships.FindAsync(command.GymMembershipId, cancellationToken);

        if(membership is null)
        {
            return Result.NotFound(nameof(GymMembership));
        }

        if (membership.GymId != gymEmployment.GymId)
        {
            return Result.Forbidden();
        }

        membership.Status = command.NewStatus;

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
