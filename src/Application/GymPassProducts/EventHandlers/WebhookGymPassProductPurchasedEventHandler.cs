using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Models;
using FitPass.Application.GymMembershipPasses.DTOs;
using FitPass.Domain.Events.GymPassProducts;
using FitPass.Infrastructure.Localization.Resources;

namespace FitPass.Application.GymPassProducts.EventHandlers;

public class WebhookGymPassProductPurchasedEventHandler : INotificationHandler<WebhookGymPassProductPurchasedEvent>
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;
    private readonly ILocalizer _localizer;
    private readonly IClientNotificationSender  _clientNotificationSender;

    public WebhookGymPassProductPurchasedEventHandler(
        IApplicationDbContext context,
        TimeProvider timeProvider,
        ILocalizer  localizer,
        IClientNotificationSender clientNotificationSender)
    {
        _context = context;
        _timeProvider = timeProvider;
        _localizer = localizer;
        _clientNotificationSender = clientNotificationSender;
    }
    
    public async Task Handle(WebhookGymPassProductPurchasedEvent notification, CancellationToken cancellationToken)
    {   
        var passes = await _context.GymMembershipPasses
            .Where(x => x.UserId == notification.UserId)
            .ToListAsync(cancellationToken);

        var dtos = passes
            .Where(x => x.IsValid(_timeProvider.GetUtcNow()))
            .Select(x => x.MapToDto());

        var clientNotification = ClientNotification.Create(
            _localizer.Get(nameof(SharedResource.SuccessfulPurchase)),
            ClientNotificationType.SuccessfulPurchase,
            dtos);
        
        await _clientNotificationSender.SendAsync(notification.UserId, clientNotification);
    }
}
