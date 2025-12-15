using FitPass.Application.Common.Models;
using FitPass.Application.Webhooks.Products;
using Microsoft.Extensions.Logging;
using Stripe;

namespace FitPass.Infrastructure.Stripe.Services.Webhook;

public partial class StripeWebhookService
{
    private Result<(string accountId, Product product)> GetProductEventData(Event stripeEvent)
    {
        if (stripeEvent.Data.Object is not Product product)
        {
            _logger.LogError("Product data is null in webhook StripeEvent {StripeEvent}", stripeEvent);

            return Result.BusinessRuleViolation("Invalid request.");
        }

        return Result.Success((stripeEvent.Account, product));
    }

    private async Task<Result> HandleProductUpdated(Event stripeEvent)
    {
        var result = GetProductEventData(stripeEvent);

        if (!result.Succeeded)
        {
            return result;
        }

        var product = result.Value.product;

        return await _sender.Send(
            new SyncPaymentProviderProductUpdateCommand(product.Id, product.Name, product.Description, product.Active, result.Value.accountId));
    }

    private async Task<Result> HandleProductCreated(Event stripeEvent)
    {
        var result = GetProductEventData(stripeEvent);

        if (!result.Succeeded)
        {
            return result;
        }

        return await _sender.Send(new SyncPaymentProviderProductCreatedCommand(result.Value.product.Id, result.Value.accountId));
    }

    private async Task<Result> HandleProductDeleted(Event stripeEvent)
    {
        var result = GetProductEventData(stripeEvent);

        if (!result.Succeeded)
        {
            return result;
        }

        return await _sender.Send(new SyncPaymentProviderProductDeletedCommand(result.Value.product.Id, result.Value.accountId));
    }
}
