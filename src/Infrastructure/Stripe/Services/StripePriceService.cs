using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Models;
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

    public async Task<Result> CreatePrice(GymPassProduct gymPassProduct, CancellationToken cancellationToken)
    {
        try
        {
            var priceOptions = new PriceCreateOptions
            {
                Product = gymPassProduct.Id,
                Currency = _settings.Currency,
                UnitAmountDecimal = gymPassProduct.HUFPrice
            };

            cancellationToken.ThrowIfCancellationRequested();

            var price = await _priceService.CreateAsync(priceOptions, null, cancellationToken);

            gymPassProduct.HasPriceOnStripe = true;
            gymPassProduct.StripePriceId = price.Id;

            await _context.SaveChangesAsync();

            return Result.Success();
        } catch (StripeException ex)
        {
            return ex.LogAndGetResult<StripePriceService>(_logger);
        }
    }

    public async Task<Result> ArchivePrice(GymPassProduct gymPassProduct, CancellationToken cancellationToken)
    {
        try
        {
            if (!gymPassProduct.HasPriceOnStripe || gymPassProduct.StripePriceId == null)
            {
                return Result.Failure(["Price does not exist for this product."]);
            }

            var priceOptions = new PriceUpdateOptions
            {
                Active = false
            };

            cancellationToken.ThrowIfCancellationRequested();

            await _priceService.UpdateAsync(gymPassProduct.StripePriceId, priceOptions, cancellationToken: cancellationToken);

            gymPassProduct.HasPriceOnStripe = false;
            gymPassProduct.StripePriceId = null;

            await _context.SaveChangesAsync();

            return Result.Success();
        } catch (StripeException ex)
        {
            return ex.LogAndGetResult<StripePriceService>(_logger);
        }
    }
}
