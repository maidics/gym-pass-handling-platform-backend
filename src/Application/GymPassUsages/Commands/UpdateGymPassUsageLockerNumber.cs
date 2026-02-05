using FitPass.Application.Common.Extensions;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Models;
using FitPass.Application.Common.Resources;
using FitPass.Application.Common.Scopes;
using FitPass.Application.Common.Security;
using FitPass.Domain.Constants;

namespace FitPass.Application.GymPassUsages.Commands;

[Authorize(Roles = $"{Roles.GymAdministrator},{Roles.GymStaff}")]
public record UpdateGymPassUsageLockerNumberCommand(string GymPassUsageId, string LockerNumber)
    : IRequest<Result>;

public class UpdateGymPassUsageLockerNumberCommandValidator
    : AbstractValidator<UpdateGymPassUsageLockerNumberCommand>
{
    public UpdateGymPassUsageLockerNumberCommandValidator(ILocalizer localizer)
    {
        RuleFor(v => v.GymPassUsageId)
            .PropertyOfEntityNotEmptyWithMessageLocalized(
                localizer,
                nameof(SharedResource.Id),
                nameof(SharedResource.GymPassUsage)
            );

        RuleFor(v => v.LockerNumber)
            .NotEmptyWithMessageLocalized(localizer, nameof(SharedResource.LockerNumber));
    }
}

public class UpdateGymPassUsageLockerNumberCommandHandler
    : IRequestHandler<UpdateGymPassUsageLockerNumberCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly ILocalizer _localizer;
    private readonly IClientNotificationSender _clientNotificationSender;

    public UpdateGymPassUsageLockerNumberCommandHandler(
        IApplicationDbContext context,
        ILocalizer localizer,
        IClientNotificationSender clientNotificationSender
    )
    {
        _context = context;
        _localizer = localizer;
        _clientNotificationSender = clientNotificationSender;
    }

    public async Task<Result> Handle(
        UpdateGymPassUsageLockerNumberCommand command,
        CancellationToken cancellationToken
    )
    {
        var gymPassUsage = await _context.GymPassUsages.FindAsync(
            [command.GymPassUsageId],
            cancellationToken
        );

        if (gymPassUsage is null)
        {
            return Result.NotFound(_localizer.GetNotFound(nameof(SharedResource.GymPassUsage)));
        }

        if (gymPassUsage.HasGymSessionEnded())
        {
            return Result.BusinessRuleViolation(
                _localizer.Get(nameof(SharedResource.NoChangingLockerNumberAfterGymSessionEnded))
            );
        }

        gymPassUsage.LockerNumber = command.LockerNumber;

        await _context.SaveChangesAsync(cancellationToken);

        var lang = await _context
            .UserProfiles.AsNoTracking()
            .Where(x => x.UserId == gymPassUsage.UserId)
            .Select(x => x.PreferredLanguage)
            .FirstOrDefaultAsync(cancellationToken);

        using var scope = new CultureInfoScope(lang ?? _localizer.DefaultCulture);

        var notification = new ClientNotification
        {
            Message = _localizer.Get(
                nameof(SharedResource.GymEmployeeUpdatedGymPassUsageLockerNumber),
                command.LockerNumber
            ),
            Type = ClientNotificationType.GymPassUsageLockerUpdated,
        };

        await _clientNotificationSender.SendAsync(gymPassUsage.UserId, notification);

        return Result.Success();
    }
}
