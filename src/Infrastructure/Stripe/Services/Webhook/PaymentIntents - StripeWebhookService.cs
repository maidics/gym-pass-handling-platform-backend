using FitPass.Application.Common.Models;
using FitPass.Application.GymPassProducts.Commands;
using FitPass.Application.Webhooks;
using Microsoft.Extensions.Logging;
using Stripe;

namespace FitPass.Infrastructure.Stripe.Services.Webhook;

public partial class StripeWebhookService
{
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
