using FitPass.Application.Common.Interfaces;
using FitPass.Domain.Events.GymPassProducts;
using Microsoft.Extensions.Logging;

namespace FitPass.Application.GymPassProducts.EventHandlers;

public class GymPassProductPurchaseFulfillmentFailedEventHandler : INotificationHandler<GymPassProductPurchaseFulfillmentFailedEvent>
{
    //TODO: check archive db context here, if it exists in the archive add it to the user, if not send failure email

    private readonly IEmailService _emailService;
    private readonly ILogger<GymPassProductPurchaseFulfillmentFailedEventHandler> _logger;

    public GymPassProductPurchaseFulfillmentFailedEventHandler(
        IEmailService emailService,
        ILogger<GymPassProductPurchaseFulfillmentFailedEventHandler> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }
    

    public Task Handle(GymPassProductPurchaseFulfillmentFailedEvent notification, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
