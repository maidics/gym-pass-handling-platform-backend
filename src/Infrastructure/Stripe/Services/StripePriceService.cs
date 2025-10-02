using FitPass.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace Fitpass.Infrastructure.Stripe.Services;

public class StripePriceService : IStripePriceService
{
    private readonly ILogger<StripePriceService> _logger;

    public StripePriceService(ILogger<StripePriceService> logger)
    {
        _logger = logger;
    }
}