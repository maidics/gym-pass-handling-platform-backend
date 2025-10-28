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

    public async Task CreateProduct(GymPassProduct gymPassProduct)
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

            var product = await _productService.CreateAsync(productOptions, null);

            gymPassProduct.IsCreatedOnStripe = true;
        } catch (StripeException ex)
        {
            ex.LogAndGetApplicationException<StripeProductService>(_logger);
        }
    }
}
