//
// using FitPass.Application.Common.Models;
// using FitPass.Application.Webhooks.Prices;
// using Microsoft.Extensions.Logging;
// using Stripe;
//
// namespace FitPass.Infrastructure.Stripe.Services.Webhook;
//
// public partial class StripeWebhookService
// {
//     private Result<(string accountId, Price price)> GetPriceEventData(Event stripeEvent)
//     {
//         if (stripeEvent.Data.Object is not Price price)
//         {
//             _logger.LogError("Price data is null in webhook StripeEvent {StripeEvent}", stripeEvent);
//
//             return Result.BusinessRuleViolation("Invalid request.");
//         }
//
//         return Result.Success((stripeEvent.Account, price));
//     }
//
//     private async Task<Result> HandlePriceCreated(Event stripeEvent)
//     {
//         var result = GetPriceEventData(stripeEvent);
//
//         if (!result.Succeeded)
//         {
//             return result;
//         }
//
//         return await _sender.Send(new SyncPaymentProviderPriceCreatedCommand(result.Value.price.Id, result.Value.accountId));
//     }
//
//     private async Task<Result> HandlePriceUpdated(Event stripeEvent)
//     {
//         var result = GetPriceEventData(stripeEvent);
//
//         if (!result.Succeeded)
//         {
//             return result;
//         }
//
//         return await _sender.Send(new SyncPaymentProviderPriceUpdatedCommand(result.Value.price.Id, result.Value.accountId));
//     }
//
//     private async Task<Result> HandlePriceDeleted(Event stripeEvent)
//     {
//         var result = GetPriceEventData(stripeEvent);
//
//         if (!result.Succeeded)
//         {
//             return result;
//         }
//
//         return await _sender.Send(new SyncPaymentProviderPriceDeletedCommand(result.Value.price.Id, result.Value.accountId));
//     }
// }
