using FitPass.Application.Common.Constants;
using FitPass.Application.Common.Extensions;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Models;
using FitPass.Application.Common.Security;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using FitPass.Domain.Enums;
using FitPass.Domain.Events.GymMemberships;

namespace FitPass.Application.GymMemberships.Commands;

[Authorize(Roles = $"{Roles.GymAdministrator},{Roles.GymStaff}")]
public record UpdateGymMembershipStatusCommand(string GymMembershipId, GymMembershipStatus NewStatus) : IRequest<Result>;

public class UpdateGymMembershipStatusCommandValidator : AbstractValidator<UpdateGymMembershipStatusCommand>
{
    public UpdateGymMembershipStatusCommandValidator(ILocalizer localizer)
    {
        RuleFor(v => v.GymMembershipId)
            .NotEmptyLocalized(localizer, LocalizationKeys.GymMembershipId);
    }
}

public class UpdateGymMembershipStatusCommandHandler : IRequestHandler<UpdateGymMembershipStatusCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly ILocalizer _localizer;

    public UpdateGymMembershipStatusCommandHandler(
        IApplicationDbContext context,
        IUser user,
        ILocalizer localizer)
    {
        _context = context;
        _user = user;
        _localizer = localizer;
    }
    public async Task<Result> Handle(UpdateGymMembershipStatusCommand command, CancellationToken cancellationToken)
    {
        var gymEmployment = await _context.GymEmployments
            .AsNoTracking()
            .FirstOrDefaultAsync(gsa => gsa.UserId == _user.Id, cancellationToken);

        Guard.Against.NullParameterRelatedToCurrentUser(gymEmployment, nameof(GymEmployment), _user.Id);

        var membership = await _context.GymMemberships.FindAsync(command.GymMembershipId, cancellationToken);

        if(membership is null)
        {
            return Result.NotFound(_localizer.Get(LocalizationKeys.NotFound, nameof(GymMembership)));
        }

        if (membership.GymId != gymEmployment.GymId)
        {
            return Result.Forbidden(_localizer.Get(LocalizationKeys.Forbidden));
        }

        if (membership.Status == command.NewStatus)
        {
            return Result.Success();
        }

        membership.AddDomainEvent(new GymMembershipStatusChangedEvent
        {
            UserId = membership.UserId,
            NewStatus = command.NewStatus,
            GymId = membership.GymId
        });
        
        await _context.SaveChangesAsync(cancellationToken);
        
        return Result.Success();
    }
}
