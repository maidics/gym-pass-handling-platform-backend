using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Interfaces.Payment;
using FitPass.Application.Common.Models;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Stripe;

namespace FitPass.Infrastructure.Stripe.Services.Webhook;
public partial class StripeWebhookService : IPaymentWebhookService
{
    private readonly ILogger<StripeWebhookService> _logger;
    private readonly ISender _sender;
    private readonly string _webhookSecret;
    private readonly IClientNotificationSender _notificationSender;

    public StripeWebhookService(
        ILogger<StripeWebhookService> logger,
        ISender sender,
        IConfiguration configuration,
        IClientNotificationSender notificationSender)
    {
        _logger = logger;
        _sender = sender;
        _webhookSecret = configuration["Stripe:WebHookSecret"] ?? throw new InvalidOperationException("Stripe webhook secret is not configured");
        _notificationSender = notificationSender;
    }

    public Task<Result> ProcessAsync(string json, string signature, CancellationToken cancellationToken = default)
    {
        var stripeEvent = EventUtility.ConstructEvent(json, signature, _webhookSecret);

        return stripeEvent.Type switch
        {
            EventTypes.AccountUpdated => HandleAccountUpdated(stripeEvent),
            EventTypes.PaymentIntentSucceeded => HandlePaymentIntentSucceeded(stripeEvent),
            EventTypes.PaymentIntentCanceled => HandlePaymentIntentCanceled(stripeEvent),
            EventTypes.PaymentIntentPaymentFailed =>  HandlePaymentIntentPaymentFailed(stripeEvent),
            EventTypes.PaymentIntentRequiresAction => HandlePaymentIntentRequiresAction(stripeEvent),
            EventTypes.PaymentIntentProcessing => HandlePaymentIntentProcessing(stripeEvent),
            _ => HandleUnhandled(stripeEvent)
        };
    }

    private Task<Result> HandleUnhandled(Event stripeEvent)
    {
        _logger.LogError("Unhandled Stripe Webhook event: {StripeEvent}.", stripeEvent);

        return Task.FromResult(Result.Failure("Unhandled payment provider webhook.", [], ResultTypes.InternalError));
    }

    private Result<T> GetEventData<T>(Event stripeEvent)
    {
        if (stripeEvent.Data.Object is not T instance)
        {
            _logger.LogError("{T} data is null in webhook StripeEvent {StripeEvent}", typeof(T), stripeEvent);

            return Result.BusinessRuleViolation();
        }

        return Result.Success(instance);
    }
}
