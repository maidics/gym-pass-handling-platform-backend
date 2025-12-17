using FitPass.Application.Common.EmailModels.Gyms;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Models;
using FitPass.Domain.Events.Gyms;

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

        var model = new GymStatusUpdatedByAppAdminEmailModel
        {
            Language = _localizer.DefaultCulture, 
            NewStatus = notification.NewStatus,
            GymName = notification.GymName
        };
        
        await _emailService.SendEmailAsync(model,  gymEmployeeEmails);
    }
}
