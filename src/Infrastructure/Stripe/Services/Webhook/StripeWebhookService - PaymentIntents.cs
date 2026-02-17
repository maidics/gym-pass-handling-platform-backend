using FitPass.Application.Common.Models;
using FitPass.Application.GymPassProducts.Commands;
using Microsoft.Extensions.Logging;
using Stripe;

namespace FitPass.Infrastructure.Stripe.Services.Webhook;

public partial class StripeWebhookService
{
    private async Task<Result> HandlePaymentIntentSucceededAsync(Event stripeEvent)
    {
        var result = GetPaymentIntentEventData(stripeEvent);

        if (!result.Succeeded)
        {
            return result;
        }

        var (userId, gymId, gymPassProductId) = result.Value;

        return await _sender.Send(
            new WebhookFulFillGymPassProductPaymentCommand(userId, gymId, gymPassProductId),
            CancellationToken.None
        );
    }

    private async Task<Result> HandlePaymentIntentCanceledAsync(Event stripeEvent)
    {
        var result = GetPaymentIntentEventData(stripeEvent);

        if (!result.Succeeded)
        {
            return result;
        }

        var notification = new ClientNotification
        {
            Message = "Payment was canceled.",
            Type = ClientNotificationType.Default,
        };

        await _notificationSender.SendAsync(result.Value.userId, notification);

        return Result.Success();
    }

    private async Task<Result> HandlePaymentIntentPaymentFailedAsync(Event stripeEvent)
    {
        var result = GetPaymentIntentEventData(stripeEvent);

        if (!result.Succeeded)
        {
            return result;
        }

        var notification = new ClientNotification
        {
            Message = "Payment has failed. Not enough funds or card was declined",
            Type = ClientNotificationType.PaymentFailed,
        };

        await _notificationSender.SendAsync(result.Value.userId, notification);

        return Result.Success();
    }

    private async Task<Result> HandlePaymentIntentRequiresActionAsync(Event stripeEvent)
    {
        var result = GetPaymentIntentEventData(stripeEvent);

        if (!result.Succeeded)
        {
            return result;
        }

        var notification = new ClientNotification
        {
            Message = "Payment requires action. Please check your banking application.",
            Type = ClientNotificationType.Default,
        };

        await _notificationSender.SendAsync(result.Value.userId, notification);

        return Result.Success();
    }

    private Task<Result> HandlePaymentIntentCreatedAsync(Event stripeEvent)
    {
        var result = GetPaymentIntentEventData(stripeEvent);

        if (!result.Succeeded)
        {
            return Task.FromResult<Result>(result);
        }

        var data = result.Value;

        _logger.LogInformation(
            "Payment intent created. UserId: {UserId}, GymId: {GymId}, GymPassProductId: {GymPassProductId}",
            data.userId,
            data.gymId,
            data.gymPassProductId
        );

        return Task.FromResult(Result.Success());
    }

    /* unused:
    private async Task<Result> HandlePaymentIntentProcessing(Event stripeEvent)
    {
        var result = GetPaymentIntentEventData(stripeEvent);

        if (!result.Succeeded)
        {
            return result;
        }

        var notification = new ClientNotification
        {
            Message = "Processing your payment...",
            Type = ClientNotificationType.Default,
        };

        await _notificationSender.SendAsync(result.Value.userId, notification);

        return Result.Success();
    }
    */
}
