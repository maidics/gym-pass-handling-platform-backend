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
        IClientNotificationSender notificationSender
    )
    {
        _logger = logger;
        _sender = sender;
        _webhookSecret =
            configuration["Stripe:WebHookSecret"]
            ?? throw new InvalidOperationException("Stripe webhook secret is not configured");
        _notificationSender = notificationSender;
    }

    public Task<Result> ProcessAsync(
        string json,
        string signature,
        CancellationToken cancellationToken = default
    )
    {
        var stripeEvent = EventUtility.ConstructEvent(json, signature, _webhookSecret);

        return stripeEvent.Type switch
        {
            EventTypes.PaymentIntentCreated => HandlePaymentIntentCreatedAsync(stripeEvent),
            EventTypes.PaymentIntentSucceeded => HandlePaymentIntentSucceededAsync(stripeEvent),
            EventTypes.PaymentIntentCanceled => HandlePaymentIntentCanceledAsync(stripeEvent),
            EventTypes.PaymentIntentPaymentFailed => HandlePaymentIntentPaymentFailedAsync(
                stripeEvent
            ),
            EventTypes.PaymentIntentRequiresAction => HandlePaymentIntentRequiresActionAsync(
                stripeEvent
            ),

            { } t when t.StartsWith("charge", StringComparison.OrdinalIgnoreCase) =>
                HandleChargeEventAsync(stripeEvent),

            /*
            EventTypes.AccountUpdated => HandleAccountUpdated(stripeEvent),
            EventTypes.PaymentIntentProcessing => HandlePaymentIntentProcessing(stripeEvent),
            EventTypes.ProductCreated => HandleProductCreated(stripeEvent),
            EventTypes.ProductDeleted => HandleProductDeleted(stripeEvent),
            EventTypes.ProductUpdated => HandleProductUpdated(stripeEvent),
            EventTypes.PriceCreated => HandlePriceCreated(stripeEvent),
            EventTypes.PriceUpdated => HandlePriceUpdated(stripeEvent),
            EventTypes.PriceDeleted => HandlePriceDeleted(stripeEvent),
            */

            _ => HandleUnhandled(stripeEvent),
        };
    }

    private Task<Result> HandleUnhandled(Event stripeEvent)
    {
        _logger.LogError("Unhandled Stripe Webhook event type: {Type}", stripeEvent.Type);

        throw new NotImplementedException($"Unhandled Stripe event type: {stripeEvent.Type}");
    }

    // private Result<T> GetEventData<T>(Event stripeEvent)
    // {
    //     if (stripeEvent.Data.Object is not T instance)
    //     {
    //         _logger.LogError("{T} data is null in webhook StripeEvent {StripeEvent}", typeof(T), stripeEvent);
    //
    //         return Result.BusinessRuleViolation("Invalid request.");
    //     }
    //
    //     return Result.Success(instance);
    // }

    //Payment Intents:
    private Result<(
        string userId,
        string gymId,
        string gymPassProductId
    )> GetPaymentIntentEventData(Event stripeEvent)
    {
        if (stripeEvent.Data.Object is not PaymentIntent intent)
        {
            _logger.LogError(
                "Payment intent data is null in webhook PaymentIntent type Event: {StripeEvent}",
                stripeEvent
            );

            return Result.BusinessRuleViolation("Invalid payload.");
        }

        var userId = intent.Metadata.GetValueOrDefault("UserId");
        var gymId = intent.Metadata.GetValueOrDefault("GymId");
        var gymPassProductId = intent.Metadata.GetValueOrDefault("GymPassProductId");

        if (userId is null || gymId is null || gymPassProductId is null)
        {
            _logger.LogError(
                "Required metadata not found in PaymentIntent event. PaymentIntentId: {PaymentIntentId}, UserId: {UserId}, GymId: {GymId}, GymPassProductId: {GymPassProductId}",
                intent.Id,
                userId,
                gymId,
                gymPassProductId
            );

            return Result.BusinessRuleViolation("");
        }

        return Result.Success((userId, gymId, gymPassProductId));
    }
}
