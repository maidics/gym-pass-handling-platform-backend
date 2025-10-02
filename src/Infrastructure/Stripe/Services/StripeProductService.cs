using FitPass.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace Fitpass.Infrastructure.Stripe.Services;

public class StripeProductService : IStripeProductService
{
    private readonly ILogger<StripeProductService> _logger;

    public StripeProductService(ILogger<StripeProductService> logger)
    {
        _logger = logger;
    }
}