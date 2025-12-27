using FitPass.Application.Common.EmailModels.Gyms;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Resources;
using FitPass.Domain.Enums;
using FitPass.Domain.Events.Gyms;
using FitPass.Domain.Strings;

namespace FitPass.Application.Gyms.EventHandlers;

public class GymStatusUpdatedByAppAdminEventHandler : INotificationHandler<GymStatusUpdatedByAppAdminEvent>
{
    private readonly IEmailService _emailService;
    private readonly ILocalizer  _localizer;
    private readonly IQueryService _queryService;

    public GymStatusUpdatedByAppAdminEventHandler(
        IEmailService emailService,
        ILocalizer localizer,
        IQueryService queryService)
    {
        _emailService = emailService;
        _localizer = localizer;
        _queryService = queryService;
    }
    
    public async Task Handle(GymStatusUpdatedByAppAdminEvent notification, CancellationToken cancellationToken)
    {
        //TODO: send to escalation emails and other gym contacts as well?
        var gymEmployeeEmails = await _queryService.GetGymEmployeeEmailsByGymIdAsync(notification.GymId);

        var isSuspended = notification.NewStatus == GymStatus.Suspended;

        var model = new GymStatusUpdatedByAppAdminEmailModel
        {
            Language = _localizer.DefaultCulture, 

            Subject = isSuspended ? 
                _localizer.Get(nameof(SharedResource.GymSuspendedByAppAdminEmailSubject)) : 
                _localizer.Get(nameof(SharedResource.GymReactivatedByAppAdminEmailSubject)),
            
            Greeting = $"{_localizer.Get(nameof(SharedResource.EmailGreeting), notification.GymName)} {_localizer.Get(nameof(SharedResource.Team))}",

            Body = isSuspended ? 
                _localizer.Get(nameof(SharedResource.GymSuspendedByAppAdminEmailBody), notification.Rationale) : 
                _localizer.Get(nameof(SharedResource.GymReactivatedByAppAdminEmailBody), notification.Rationale),

            Farewell = _localizer.Get(nameof(SharedResource.EmailFarewell), CommonStrings.AppName)
        };
        
        await _emailService.SendEmailAsync(model,  gymEmployeeEmails);
    }
}
