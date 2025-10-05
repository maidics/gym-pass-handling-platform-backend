using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Models;
using FitPass.Domain.Entities;
using FitPass.Domain.Enums;
using FitPass.Infrastructure.Stripe;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Stripe;

namespace Fitpass.Infrastructure.Stripe.Services;

public class StripeProductService : IStripeProductService
{
    private readonly StripeSettings _settings;
    private readonly ILogger<StripeProductService> _logger;
    private readonly ProductService _productService;
    private readonly IApplicationDbContext _context;

    public StripeProductService(IOptions<StripeSettings> options, ILogger<StripeProductService> logger, ProductService productService, IApplicationDbContext context)
    {
        _settings = options.Value;
        _logger = logger;
        _productService = productService;
        _context = context;
    }

    public async Task<Result> CreateProduct(GymPassProduct gymPassProduct, CancellationToken cancellationToken)
    {
        try
        {
            var productOptions = new ProductCreateOptions
            {
                Id = gymPassProduct.Id,
                Name = gymPassProduct.Name,
                Description = gymPassProduct.Description,
                Shippable = false,
                TaxCode = gymPassProduct.Type == PassType.SingleUse ? _settings.TaxCodeSettings.SingleUseAccess : _settings.TaxCodeSettings.Membership,
                Type = "service"
            };

            cancellationToken.ThrowIfCancellationRequested();

            var product = await _productService.CreateAsync(productOptions, null, cancellationToken);

            gymPassProduct.IsCreatedOnStripe = true;

            await _context.SaveChangesAsync();

            return Result.Success();
        } catch (StripeException ex)
        {
            return ex.LogAndGetResult<StripeProductService>(_logger);
        }
    }
}
