using FitPass.Application.Common.Interfaces;
using FitPass.Domain.Entities;
using FitPass.Infrastructure.Stripe;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Stripe;

namespace Fitpass.Infrastructure.Stripe.Services;

public class StripePriceService : IStripePriceService
{
    private readonly StripeSettings _settings;
    private readonly ILogger<StripePriceService> _logger;
    private readonly PriceService _priceService;
    private readonly IApplicationDbContext _context;

    public StripePriceService(IOptions<StripeSettings> options, ILogger<StripePriceService> logger, PriceService priceService, IApplicationDbContext context)
    {
        _settings = options.Value;
        _logger = logger;
        _priceService = priceService;
        _context = context;
    }

    public async Task CreatePrice(GymPassProduct gymPassProduct, CancellationToken cancellationToken)
    {
        var priceOptions = new PriceCreateOptions
        {
            Product = gymPassProduct.Id,
            Currency = _settings.Currency,
            UnitAmountDecimal = gymPassProduct.HUFPrice
        };

        var price = await _priceService.CreateAsync(priceOptions, null, cancellationToken);
    }
}
