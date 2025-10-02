using FitPass.Application.Common.Interfaces;
using FitPass.Domain.Entities;
using FitPass.Domain.Enums;
using FitPass.Infrastructure.Stripe;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Stripe;

namespace Fitpass.Infrastructure.Stripe;

public class PaymentService : IPaymentService
{
    private readonly ILogger<PaymentService> _logger;
    private readonly IApplicationDbContext _context;
    private readonly PriceService _priceService;
    private readonly ProductService _productService;
    private readonly string _membershipTaxCode;
    private readonly string _singleUseAccessTaxCode;

    public PaymentService(ILogger<PaymentService> logger, IConfiguration configuration, IApplicationDbContext context, PriceService priceService, ProductService productService, CustomerService customerService)
    {
        var stripeSettings = configuration.GetSection("Stripe");

        var apiKey = stripeSettings["APIKey"];
        var memberShipTaxCode = stripeSettings["TaxCodes:Membership"];
        var singleUseAccessTaxCode = stripeSettings["TaxCodes:SingleUseAccess"];

        if (apiKey == null || memberShipTaxCode == null || singleUseAccessTaxCode == null)
        {
            throw new Exception("Internal server error occured.");
        }

        StripeConfiguration.ApiKey  = apiKey;

        _logger = logger;
        _context = context;

        _priceService = priceService;
        _productService = productService;

        _membershipTaxCode = memberShipTaxCode;
        _singleUseAccessTaxCode = singleUseAccessTaxCode;
    }

    public async Task CreateProduct(GymPassProduct gymPassProduct, CancellationToken cancellationToken)
    {
        try
        {
            //TODO: check & add options on deployment
            var productOptions = new ProductCreateOptions
            {
                Name = gymPassProduct.Name,
                Description = gymPassProduct.Description,
                Active = gymPassProduct.IsActive,
                /*
                DefaultPriceData = new ProductDefaultPriceDataOptions
                {
                    Currency = "huf",
                    UnitAmountDecimal = gymPassProduct.HUFPrice
                },
                */
                Shippable = false,
                TaxCode = gymPassProduct.Type == PassType.SingleUse ? _singleUseAccessTaxCode : _membershipTaxCode,
                Type = "service"
            };

            var product = await _productService.CreateAsync(productOptions, null, cancellationToken);

            Guard.Against.Null(product, "Product", "Failed to create gym pass product.");

            var priceOptions = new PriceCreateOptions
            {
                Product = product.Id,
                Currency = "huf",
                UnitAmountDecimal = gymPassProduct.HUFPrice
            };

            var price = await _priceService.CreateAsync(priceOptions, null, cancellationToken);

            Guard.Against.Null(price, "Price", "Failed to create gym pass product.");

            gymPassProduct.IsCreatedOnStripe = true;

            await _context.SaveChangesAsync();
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex.ToErrorString(), ex);
            throw;
        }
    }
}
