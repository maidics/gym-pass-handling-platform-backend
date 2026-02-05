using FitPass.Application.Common.EmailModels.GymPassProducts;
using FitPass.Application.Common.Extensions;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Models;
using FitPass.Application.Common.Resources;
using FitPass.Application.Common.Scopes;
using FitPass.Domain.Events.GymPassProducts;
using FitPass.Domain.Strings;

namespace FitPass.Application.GymPassProducts.EventHandlers;

public class WebhookGymPassProductPurchaseFulfillmentFailedEventHandler
    : INotificationHandler<WebhookGymPassProductPurchaseFulfillmentFailedEvent>
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
        IClientNotificationSender clientNotificationSender
    )
    {
        _identityService = identityService;
        _context = context;
        _localizer = localizer;
        _emailService = emailService;
        _clientNotificationSender = clientNotificationSender;
    }

    public async Task Handle(
        WebhookGymPassProductPurchaseFulfillmentFailedEvent notification,
        CancellationToken cancellationToken
    )
    {
        //TODO: instead of sending an email refund the user

        var email = await _identityService.GetEmailByIdAsync(notification.UserId);

        Guard.Against.NullParameterRelatedToCurrentUser(email, nameof(email), notification.UserId);

        var result = await _context
            .UserProfiles.AsNoTracking()
            .Where(x => x.UserId == notification.UserId)
            .Select(x => new { x.PreferredLanguage, x.FirstName })
            .FirstOrDefaultAsync();

        var gymName = await _context
            .Gyms.AsNoTracking()
            .Where(x => x.Id == notification.GymId)
            .Select(x => x.Name)
            .FirstOrDefaultAsync();

        var language = result?.PreferredLanguage ?? _localizer.DefaultCulture;

        GymPassProductPurchaseFulfillmentFailedEmailModel model;

        using (var scope = new CultureInfoScope(language))
        {
            model = new GymPassProductPurchaseFulfillmentFailedEmailModel
            {
                Language = language,
                Subject = _localizer.Get(
                    nameof(SharedResource.GymPassProductPurchaseFulfillmentFailedEmailSubject)
                ),
                Greeting = _localizer.Get(
                    nameof(SharedResource.EmailGreeting),
                    result?.FirstName ?? nameof(SharedResource.User)
                ),
                Body = _localizer.Get(
                    nameof(SharedResource.GymPassProductPurchaseFulfillmentFailedEmailBody),
                    gymName ?? nameof(SharedResource.Gym),
                    notification.ReceiptId
                ),
                Farewell = _localizer.Get(
                    nameof(SharedResource.EmailFarewell),
                    CommonStrings.AppName
                ),
            };
        }

        await _emailService.SendEmailAsync(model, email);

        var clientNotification = new ClientNotification
        {
            Message = _localizer.Get(nameof(SharedResource.FailedToFulfillGymPassProductPayment)),
            Type = ClientNotificationType.GymPassProductPurchaseFulfillmentFailed,
        };

        await _clientNotificationSender.SendAsync(notification.UserId, clientNotification);
    }
}
