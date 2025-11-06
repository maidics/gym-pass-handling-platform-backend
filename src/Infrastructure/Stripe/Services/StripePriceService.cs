using FitPass.Application.Common.Interfaces;
using FitPass.Domain.Entities;
using FitPass.Infrastructure.Stripe;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Stripe;

namespace FitPass.Infrastructure.Stripe.Services;

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

    public async Task CreatePrice(GymPassProduct gymPassProduct)
    {
        try
        {
            var priceOptions = new PriceCreateOptions
            {
                Product = gymPassProduct.Id,
                Currency = _settings.Currency,
                UnitAmountDecimal = gymPassProduct.HufPrice
            };

            var price = await _priceService.CreateAsync(priceOptions, null);

            gymPassProduct.HasPriceOnStripe = true;
            gymPassProduct.StripePriceId = price.Id;
        } catch (StripeException ex)
        {
            throw ex.LogAndGetApplicationException(_logger);
        }
    }

    public async Task ArchivePrice(GymPassProduct gymPassProduct)
    {
        try
        {
            if (!gymPassProduct.HasPriceOnStripe || gymPassProduct.StripePriceId == null)
            {
                _logger.LogWarning("Attempted to archive GymPassProduct with '{GymPassProductId}' that has no price on Stripe.", gymPassProduct.Id);
                return;
            }

            var priceOptions = new PriceUpdateOptions
            {
                Active = false
            };

            await _priceService.UpdateAsync(gymPassProduct.StripePriceId, priceOptions);

            gymPassProduct.HasPriceOnStripe = false;
            gymPassProduct.StripePriceId = null;
        } catch (StripeException ex)
        {
            ex.LogAndGetApplicationException(_logger);
        }
    }
}
