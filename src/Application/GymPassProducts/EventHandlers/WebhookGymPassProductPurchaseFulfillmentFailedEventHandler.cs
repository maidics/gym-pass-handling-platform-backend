using FitPass.Application.Common.EmailModels.GymPassProducts;
using FitPass.Application.Common.Extensions;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Models;
using FitPass.Domain.Events.GymPassProducts;
using FitPass.Infrastructure.Localization.Resources;

namespace FitPass.Application.GymPassProducts.EventHandlers;

public class WebhookGymPassProductPurchaseFulfillmentFailedEventHandler : INotificationHandler<WebhookGymPassProductPurchaseFulfillmentFailedEvent>
{
    private readonly IIdentityService _identityService;
    private readonly IApplicationDbContext _context;
    private readonly ILocalizer _localizer;
    private readonly IEmailService _emailService;
    private readonly IClientNotificationSender _clientNotificationSender;

    public WebhookGymPassProductPurchaseFulfillmentFailedEventHandler(
        IIdentityService identityService,
        IApplicationDbContext context,
        ILocalizer localizer,
        IEmailService emailService,
        IClientNotificationSender clientNotificationSender)
    {
        _identityService = identityService;
        _context = context;
        _localizer = localizer;
        _emailService =  emailService;
        _clientNotificationSender = clientNotificationSender;
    }
    
    public async Task Handle(WebhookGymPassProductPurchaseFulfillmentFailedEvent notification, CancellationToken cancellationToken)
    {
        //TODO: instead of sending an email refund the user
        
        var email = await _identityService.GetEmailByIdAsync(notification.UserId);
        
        Guard.Against.NullParameterRelatedToCurrentUser(email, nameof(email), notification.UserId);

        var result = await _context.UserProfiles
            .AsNoTracking()
            .Where(x => x.UserId == notification.UserId)
            .Select(x => new { x.PreferredLanguage, x.FirstName })
            .FirstOrDefaultAsync();
        
        var gymName = await _context.Gyms
            .AsNoTracking()
            .Where(x => x.Id == notification.GymId)
            .Select(x => x.Name)
            .FirstOrDefaultAsync();

        var model = new GymPassProductPurchaseFulfillmentFailedEmailModel
        {
            Language = result?.PreferredLanguage ?? _localizer.DefaultCulture,
            UserFirstName = result?.FirstName ?? _localizer.GetForCulture(result?.PreferredLanguage ?? _localizer.DefaultCulture, nameof(SharedResource.User)),
            ReceiptId = notification.ReceiptId,
            GymName = gymName ?? _localizer.Get(nameof(SharedResource.Gym))
        };

        await _emailService.SendEmailAsync(model, email);

        var clientNotification = ClientNotification.Create(
            _localizer.Get(nameof(SharedResource.FailedToFulfillGymPassProductPayment)),
            ClientNotificationType.GymPassProductPurchaseFulfillmentFailed);

        await _clientNotificationSender.SendAsync(notification.UserId, clientNotification);
    }
}
