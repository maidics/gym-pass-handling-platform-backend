using FitPass.Application.Common.Interfaces;
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
    private readonly ILocalizer _localizer;

    public StripePriceService(
        ILogger<StripePriceService> logger, 
        PriceService priceService,
        ILocalizer localizer)
    {
        _logger = logger;
        _priceService = priceService;
        _localizer = localizer;
    }

    public async Task<Result<string>> CreatePriceAsync(string productId, Money priceMoney, bool isActive, string paymentAccountId)
    {
        try
        {
            var priceOptions = new PriceCreateOptions
            {
                Product = productId,
                Currency = priceMoney.ToStripeCurrency(),
                UnitAmountDecimal = priceMoney.ToStripeAmount(),
                Active = isActive
            };

            var requestOptions = new RequestOptions
            {
                StripeAccount = paymentAccountId
            };

            var price = await _priceService.CreateAsync(priceOptions, requestOptions);

            return Result.Success(price.Id);
        } catch (StripeException ex)
        {
            ex.Log(_logger, nameof(StripePriceService), nameof(CreatePriceAsync));

            return ex.ToResultFailure(_localizer.GetExternalServiceNotAvailable("Stripe"));
        }
    }

    public async Task<Result<string>> UpdatePriceAsync(string priceId, string productId, Money newPrice, bool isActive, string paymentAccountId)
    {
        try
        {
            var priceUpdateOptions = new PriceUpdateOptions
            {
                Active = false
            };

            var requestOptions = new RequestOptions { StripeAccount = paymentAccountId };

            await _priceService.UpdateAsync(priceId, priceUpdateOptions, requestOptions: requestOptions); 
            //because prices cannot be deleted it has to be set to Active = false

            return await CreatePriceAsync(productId, newPrice, isActive, paymentAccountId);
        } catch (StripeException ex)
        {
            ex.Log(_logger, nameof(StripePriceService), nameof(UpdatePriceAsync));

            return ex.ToResultFailure(_localizer.GetExternalServiceNotAvailable("Stripe"));
        }
    }

    public async Task<Result> UpdateActiveStatusAsync(string priceId, string paymentAccountId, bool isActive)
    {
        try
        {
            var priceUpdateOptions = new PriceUpdateOptions
            {
                Active = isActive
            };

            var requestOptions = new RequestOptions() { StripeAccount = paymentAccountId };

            await _priceService.UpdateAsync(priceId, priceUpdateOptions, requestOptions);

            return Result.Success();
        } catch (StripeException ex)
        {
            ex.Log(_logger, nameof(StripePriceService), nameof(UpdateActiveStatusAsync));

            return ex.ToResultFailure(_localizer.GetExternalServiceNotAvailable("Stripe"));
        }
    }

    public Result ValidateMoney(Money money)
    {
        return money.ValidateForStripe();
    }
}
