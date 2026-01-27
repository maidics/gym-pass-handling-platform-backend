using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Interfaces.Payment;
using FitPass.Application.Common.Models;
using FitPass.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using Stripe;

namespace FitPass.Infrastructure.Stripe.Services;

public class StripePaymentService : IPaymentService
{
    private readonly PaymentIntentService _paymentIntentService;
    private readonly ILogger<StripePaymentService> _logger;
    private readonly ILocalizer _localizer;

    public StripePaymentService(
        PaymentIntentService paymentIntentService,
        ILogger<StripePaymentService> logger,
        ILocalizer localizer
    )
    {
        _paymentIntentService = paymentIntentService;
        _logger = logger;
        _localizer = localizer;
    }

    public async Task<Result<string>> CreateOneTimePaymentIntent(Money money, string userId, string gymId, string gymPassProductId, string tenantPaymentAccountId)
    {
        try
        {
            var options = new PaymentIntentCreateOptions
            {
                Amount = money.ToStripeAmount(),
                Currency = money.ToStripeCurrency(),
                AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
                {
                    Enabled = true
                },
                Metadata = new Dictionary<string, string>()
                {
                    { "UserId", userId },
                    { "GymId", gymId },
                    { "GymPassProductId", gymPassProductId }
                },
            };

            var requestOptions = new RequestOptions
            {
                StripeAccount = tenantPaymentAccountId
            };

            var intent = await _paymentIntentService.CreateAsync(options, requestOptions);

            return Result.Success(intent.ClientSecret);
        } catch (StripeException ex)
        {
            ex.Log(_logger, nameof(StripePaymentService), nameof(CreateOneTimePaymentIntent));

            return ex.ToResultFailure(_localizer.GetExternalServiceNotAvailable("Stripe"));
        }
    }
}
