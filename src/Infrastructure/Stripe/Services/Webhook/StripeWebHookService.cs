using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Interfaces.Payment;
using FitPass.Application.Common.Models;
using FitPass.Application.GymPassProducts.Commands;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Stripe;

namespace FitPass.Infrastructure.Stripe.Services.Webhook;
public class StripeWebhookService : IPaymentWebhookService
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
            EventTypes.PaymentIntentSucceeded => HandlePaymentIntentSucceeded(stripeEvent),
            EventTypes.PaymentIntentCanceled => HandlePaymentIntentCanceled(stripeEvent),
            EventTypes.PaymentIntentPaymentFailed =>  HandlePaymentIntentPaymentFailed(stripeEvent),
            EventTypes.PaymentIntentRequiresAction => HandlePaymentIntentRequiresAction(stripeEvent),
            /*EventTypes.AccountUpdated => HandleAccountUpdated(stripeEvent),
            EventTypes.PaymentIntentProcessing => HandlePaymentIntentProcessing(stripeEvent),
            EventTypes.ProductCreated => HandleProductCreated(stripeEvent),
            EventTypes.ProductDeleted => HandleProductDeleted(stripeEvent),
            EventTypes.ProductUpdated => HandleProductUpdated(stripeEvent),
            EventTypes.PriceCreated => HandlePriceCreated(stripeEvent),
            EventTypes.PriceUpdated => HandlePriceUpdated(stripeEvent),
            EventTypes.PriceDeleted => HandlePriceDeleted(stripeEvent),*/
            _ => HandleUnhandled(stripeEvent)
        };
    }

    private Task<Result> HandleUnhandled(Event stripeEvent)
    {
        _logger.LogError("Unhandled Stripe Webhook event: {StripeEvent}.", stripeEvent);

        return Task.FromResult(Result.Failure("Unhandled payment provider webhook.", [], ResultTypes.InternalError));
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
    private Result<(string userId, string gymId, string gymPassProductId)> GetPaymentIntentEventData(Event stripeEvent)
    {
        if (stripeEvent.Data.Object is not PaymentIntent intent)
        {
            _logger.LogError("Payment intent data is null in webhook StripeEvent {StripeEvent}", stripeEvent);

            return Result.BusinessRuleViolation("Invalid payload.");
        }

        var userId = intent.Metadata.GetValueOrDefault("UserId");
        var gymId = intent.Metadata.GetValueOrDefault("GymId");
        var gymPassProductId = intent.Metadata.GetValueOrDefault("GymPassProductId");

        if (userId is null || gymId is null || gymPassProductId is null)
        {
            _logger.LogError(
                "One or more required metadata inside payment intent StripeEvent {StripeEvent} was null. Found data: {UserId}, {GymId}, {GymPassProductId}",
                stripeEvent,
                userId,
                gymId,
                gymPassProductId);

            return Result.BusinessRuleViolation("Invalid request.");
        }

        return Result.Success((userId, gymId, gymPassProductId));
    }

    private async Task<Result> HandlePaymentIntentSucceeded(Event stripeEvent)
    {
        var result = GetPaymentIntentEventData(stripeEvent);

        if (!result.Succeeded)
        {
            return result;
        }

        var (userId, gymId, gymPassProductId) = result.Value;

        return await _sender.Send(new WebhookFulFillGymPassProductPaymentCommand(userId, gymId, gymPassProductId));
    }

    private async Task<Result> HandlePaymentIntentCanceled(Event stripeEvent)
    {
        var result = GetPaymentIntentEventData(stripeEvent);

        if (!result.Succeeded)
        {
            return result;
        }

        var notification = ClientNotification.Create("Payment was canceled.", ClientNotificationType.Default);

        await _notificationSender.SendAsync(result.Value.userId, notification);

        return Result.Success();
    }

    private async Task<Result> HandlePaymentIntentPaymentFailed(Event stripeEvent)
    {
        var result = GetPaymentIntentEventData(stripeEvent);

        if (!result.Succeeded)
        {
            return result;
        }

        var notification = ClientNotification.Create("Payment has failed. Not enough funds or card was declined", ClientNotificationType.PaymentFailed);

        await _notificationSender.SendAsync(result.Value.userId, notification);

        return Result.Success();
    }

    private async Task<Result> HandlePaymentIntentRequiresAction(Event stripeEvent)
    {
        var result = GetPaymentIntentEventData(stripeEvent);

        if (!result.Succeeded)
        {
            return result;
        }

        var notification = ClientNotification.Create("Payment requires action. Please check your banking application.", ClientNotificationType.Default);

        await _notificationSender.SendAsync(result.Value.userId, notification);

        return Result.Success();
    }

    private async Task<Result> HandlePaymentIntentProcessing(Event stripeEvent)
    {
        var result = GetPaymentIntentEventData(stripeEvent);

        if (!result.Succeeded)
        {
            return result;
        }

        var notification = ClientNotification.Create("Processing your payment...", ClientNotificationType.Default);

        await _notificationSender.SendAsync(result.Value.userId, notification);

        return Result.Success();
    }
}
