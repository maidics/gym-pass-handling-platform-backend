using FitPass.Application.Common.Models;
using Microsoft.Extensions.Logging;
using Stripe;

namespace FitPass.Infrastructure.Stripe.Services.Webhook;

public partial class StripeWebhookService
{
    private Task<Result> HandleChargeEventAsync(Event stripeEvent)
    {
        if (stripeEvent.Data.Object is not Charge charge)
        {
            return Task.FromResult(Result.Failure("", [], ResultTypes.BusinessRuleViolation));
        }

        var paymentIntentId = charge.PaymentIntentId;

        _logger.LogInformation(
            "Charge event received. EventType: {EventType}, ChargeId: {ChargeId}, PaymentIntentId: {PaymentIntentId}.",
            stripeEvent.Type,
            charge.Id,
            paymentIntentId
        );

        return Task.FromResult(Result.Success());
    }
}
