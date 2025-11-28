using FitPass.Application.Common.Interfaces.Payment;
using FitPass.Application.Common.Models;
using FitPass.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using Stripe;

namespace FitPass.Infrastructure.Stripe.Services;

public class StripePriceService : IPaymentPriceService
{
    private readonly ILogger<StripePriceService> _logger;
    private readonly PriceService _priceService;

    public StripePriceService(
        ILogger<StripePriceService> logger, 
        PriceService priceService)
    {
        _logger = logger;
        _priceService = priceService;
    }

    public async Task<Result<string>> CreatePriceAsync(string productId, Money priceMoney, bool isActive, string accountId)
    {
        try
        {
            var priceOptions = new PriceCreateOptions
            {
                Product = productId,
                Currency = priceMoney.Currency,
                UnitAmountDecimal = priceMoney.ToStripeAmount(),
                Active = isActive
            };

            var requestOptions = new RequestOptions
            {
                StripeAccount = accountId
            };

            var price = await _priceService.CreateAsync(priceOptions, null);

            return Result.Success(price.Id);
        } catch (StripeException ex)
        {
            ex.Log(_logger, nameof(StripePriceService), nameof(CreatePriceAsync));

            return ex.ToResultFailure("Failed to create price for pass on Stripe.");
        }
    }

    public async Task<Result<string>> UpdatePriceAsync(string priceId, string productId, Money newPrice, bool isActive, string accountId)
    {
        try
        {
            var priceUpdateOptions = new PriceUpdateOptions
            {
                Active = false
            };

            await _priceService.UpdateAsync(priceId, priceUpdateOptions);

            return await CreatePriceAsync(productId, newPrice, isActive, accountId);
        } catch (StripeException ex)
        {
            ex.Log(_logger, nameof(StripePriceService), nameof(UpdatePriceAsync));

            return ex.ToResultFailure("Failed to update price on Stripe.");
        }
    }

    public async Task<Result> UpdateActiveStatusAsync(string priceId, bool isActive)
    {
        try
        {
            var priceUpdateOptions = new PriceUpdateOptions
            {
                Active = isActive
            };

            await _priceService.UpdateAsync(priceId, priceUpdateOptions);

            return Result.Success();
        } catch (StripeException ex)
        {
            ex.Log(_logger, nameof(StripePriceService), nameof(UpdateActiveStatusAsync));

            return ex.ToResultFailure("Failed to update active flag on Stripe price.");
        }
    }

    public Result ValidateMoney(Money money)
    {
        return money.ValidateForStripe();
    }
}
