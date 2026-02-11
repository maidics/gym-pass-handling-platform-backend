using FitPass.Application.Common.EmailModels.Gyms;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Models;
using FitPass.Application.Common.Resources;
using FitPass.Domain.Enums;
using FitPass.Domain.Events.Gyms;
using FitPass.Domain.Strings;

namespace FitPass.Application.Gyms.EventHandlers;

public class GymStatusUpdatedByAppAdminEventHandler
    : INotificationHandler<GymStatusUpdatedByAppAdminEvent>
{
    private readonly IEmailService _emailService;
    private readonly ILocalizer _localizer;
    private readonly IQueryService _queryService;
    private readonly IClientNotificationSender _clientNotificationSender;
    private readonly IApplicationDbContext _context;

    public GymStatusUpdatedByAppAdminEventHandler(
        IEmailService emailService,
        ILocalizer localizer,
        IQueryService queryService,
        IClientNotificationSender clientNotificationSender,
        IApplicationDbContext context
    )
    {
        _emailService = emailService;
        _localizer = localizer;
        _queryService = queryService;
        _clientNotificationSender = clientNotificationSender;
        _context = context;
    }

    public async Task Handle(
        GymStatusUpdatedByAppAdminEvent notification,
        CancellationToken cancellationToken
    )
    {
        var gymEmployeeEmails = await _queryService.GetGymEmployeeEmailsByGymIdAsync(
            notification.GymId,
            CancellationToken.None
        );

        var isSuspended = notification.NewStatus == GymStatus.Suspended;

        var model = new GymStatusUpdatedByAppAdminEmailModel
        {
            Language = _localizer.DefaultCulture,

            Subject = isSuspended
                ? _localizer.Get(nameof(SharedResource.GymSuspendedByAppAdminEmailSubject))
                : _localizer.Get(nameof(SharedResource.GymReactivatedByAppAdminEmailSubject)),

            Greeting =
                $"{_localizer.Get(nameof(SharedResource.EmailGreeting), notification.GymName)} {_localizer.Get(nameof(SharedResource.Team))}",

            Body = isSuspended
                ? _localizer.Get(
                    nameof(SharedResource.GymSuspendedByAppAdminEmailBody),
                    notification.Rationale
                )
                : _localizer.Get(
                    nameof(SharedResource.GymReactivatedByAppAdminEmailBody),
                    notification.Rationale
                ),

            Farewell = _localizer.Get(nameof(SharedResource.EmailFarewell), CommonStrings.AppName),
        };

        await _emailService.SendEmailAsync(
            model,
            gymEmployeeEmails,
            cancellationToken: CancellationToken.None
        );

        var n = new ClientNotification
        {
            Message = isSuspended
                ? _localizer.Get(
                    nameof(SharedResource.GymSuspendedByAppAdmin),
                    notification.Rationale
                )
                : _localizer.Get(
                    nameof(SharedResource.GymReactivatedByAppAdmin),
                    notification.Rationale
                ),
            Type = ClientNotificationType.GymStatusUpdatedByAppAdmin,
        };

        var gymEmployeeIds = await _context
            .GymEmployments.Where(x => x.GymId == notification.GymId)
            .Select(x => x.UserId)
            .ToListAsync(CancellationToken.None);

        await _clientNotificationSender.SendAsync(gymEmployeeIds, n);
    }
}
