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

    public async Task<Result<string>> CreatePrice(string productId, Money priceMoney)
    {
        try
        {
            var priceOptions = new PriceCreateOptions
            {
                Product = productId,
                Currency = priceMoney.Currency,
                UnitAmountDecimal = priceMoney.ToStripeAmount()
            };

            var price = await _priceService.CreateAsync(priceOptions, null);

            return Result.Success(price.Id);
        } catch (StripeException ex)
        {
            ex.Log(_logger, nameof(StripePriceService), nameof(CreatePrice));

            return ex.ToResultFailure("Failed to create price for pass on Stripe.");
        }
    }
}
