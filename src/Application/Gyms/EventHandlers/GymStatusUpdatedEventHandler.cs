using FitPass.Application.Common.Interfaces;
using FitPass.Domain.Events.Gyms;

namespace FitPass.Application.Gyms.EventHandlers;

public class GymStatusUpdatedEventHandler : INotificationHandler<GymStatusUpdatedEvent>
{
    //efficient way of retrieving all emails of the Gym employees?
    private readonly IEmailService _emailService;
    
    public async Task Handle(GymStatusUpdatedEvent notification, CancellationToken cancellationToken)
    {
        
    }
}
