using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Models;
using FitPass.Application.Common.Resources;
using FitPass.Application.GymMembershipPasses.DTOs;
using FitPass.Domain.Events.GymPassProducts;

namespace FitPass.Application.GymPassProducts.EventHandlers;

public class WebhookGymPassProductPurchasedEventHandler
    : INotificationHandler<WebhookGymPassProductPurchasedEvent>
{
    private readonly ILocalizer _localizer;
    private readonly IClientNotificationSender _clientNotificationSender;

    public WebhookGymPassProductPurchasedEventHandler(
        ILocalizer localizer,
        IClientNotificationSender clientNotificationSender
    )
    {
        _localizer = localizer;
        _clientNotificationSender = clientNotificationSender;
    }

    public async Task Handle(
        WebhookGymPassProductPurchasedEvent notification,
        CancellationToken cancellationToken
    )
    {
        var clientNotification = new ClientNotification
        {
            Message = _localizer.Get(nameof(SharedResource.SuccessfulPurchase)),
            Type = ClientNotificationType.SuccessfulPurchase,
        };

        await _clientNotificationSender.SendAsync(notification.UserId, clientNotification);
    }
}
