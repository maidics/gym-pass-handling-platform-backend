using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Models;
using FitPass.Application.GymMembershipPasses.DTOs;
using FitPass.Domain.Events.GymPassProducts;
using FitPass.Application.Common.Resources;

namespace FitPass.Application.GymPassProducts.EventHandlers;

public class WebhookGymPassProductPurchasedEventHandler : INotificationHandler<WebhookGymPassProductPurchasedEvent>
{
    private readonly ILocalizer _localizer;
    private readonly IClientNotificationSender  _clientNotificationSender;

    public WebhookGymPassProductPurchasedEventHandler(
        ILocalizer  localizer,
        IClientNotificationSender clientNotificationSender)
    {
        _localizer = localizer;
        _clientNotificationSender = clientNotificationSender;
    }
    
    public async Task Handle(WebhookGymPassProductPurchasedEvent notification, CancellationToken cancellationToken)
    {   
        var clientNotification = ClientNotification.Create(
            _localizer.Get(nameof(SharedResource.SuccessfulPurchase)),
            ClientNotificationType.SuccessfulPurchase);
        
        await _clientNotificationSender.SendAsync(notification.UserId, clientNotification);
    }
}
