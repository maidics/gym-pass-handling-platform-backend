using FitPass.Application.Common.Extensions;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Models;
using FitPass.Application.Common.Resources;
using FitPass.Application.Common.Security;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using FitPass.Domain.Enums;
using FitPass.Domain.Events.GymMemberships;

namespace FitPass.Application.GymMemberships.Commands;

[Authorize(Roles = $"{Roles.GymAdministrator},{Roles.GymStaff}")]
public record UpdateGymMembershipStatusCommand(
    string GymMembershipId,
    GymMembershipStatus NewStatus
) : IRequest<Result>;

public class UpdateGymMembershipStatusCommandValidator
    : AbstractValidator<UpdateGymMembershipStatusCommand>
{
    public UpdateGymMembershipStatusCommandValidator(ILocalizer localizer)
    {
        RuleFor(v => v.GymMembershipId)
            .PropertyOfEntityNotEmptyWithMessageLocalized(
                localizer,
                nameof(SharedResource.Id),
                nameof(SharedResource.GymMembership)
            );
    }
}

public class UpdateGymMembershipStatusCommandHandler
    : IRequestHandler<UpdateGymMembershipStatusCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly ILocalizer _localizer;

    public UpdateGymMembershipStatusCommandHandler(
        IApplicationDbContext context,
        IUser user,
        ILocalizer localizer
    )
    {
        _context = context;
        _user = user;
        _localizer = localizer;
    }

    public async Task<Result> Handle(
        UpdateGymMembershipStatusCommand command,
        CancellationToken cancellationToken
    )
    {
        var gymId = await _context
            .GymEmployments.AsNoTracking()
            .Where(x => x.UserId == _user.Id)
            .Select(x => x.GymId)
            .FirstOrDefaultAsync(cancellationToken);

        Guard.Against.NullParameterRelatedToCurrentUser(
            gymId,
            $"{nameof(GymEmployment)}.{nameof(GymEmployment.GymId)}",
            _user.Id
        );

        var membership = await _context.GymMemberships.FirstOrDefaultAsync(
            x => x.GymId == gymId && x.Id == command.GymMembershipId,
            cancellationToken
        );

        if (membership is null)
        {
            return Result.NotFound(_localizer.GetNotFound(nameof(SharedResource.GymMembership)));
        }

        if (membership.Status == command.NewStatus)
        {
            return Result.Success();
        }

        membership.Status = command.NewStatus;

        membership.AddDomainEvent(
            new GymMembershipStatusChangedEvent(
                membership.UserId,
                command.NewStatus,
                membership.GymId
            )
        );

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
