using FitPass.Application.Common.Interfaces;
using FitPass.Domain.Entities;
using FitPass.Domain.Enums;
using Microsoft.Extensions.Configuration;
using Stripe;

namespace Fitpass.Infrastructure.Services;

public class PaymentService : IPaymentService
{
    private readonly IApplicationDbContext _context;
    private readonly PriceService _priceService;
    private readonly ProductService _productService;
    private readonly string _membershipTaxCode;
    private readonly string _singleUseAccessTaxCode;

    public PaymentService(IConfiguration configuration, IApplicationDbContext context, PriceService priceService, ProductService productService)
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

        _context = context;
        _priceService = priceService;
        _productService = productService;
        _membershipTaxCode = memberShipTaxCode;
        _singleUseAccessTaxCode = singleUseAccessTaxCode;
    }

    public async Task CreateProduct(GymPassProduct gymPassProduct, CancellationToken cancellationToken)
    {
        //TODO: check & add options on deployment
        var productOptions = new ProductCreateOptions
        {
            Id = gymPassProduct.Id,
            Name = gymPassProduct.Name,
            Description = gymPassProduct.Description,
            Active = gymPassProduct.IsActive,
            DefaultPriceData = new ProductDefaultPriceDataOptions
            {
                Currency = "huf",
                UnitAmountDecimal = gymPassProduct.HUFPrice
            },
            Shippable = false,
            TaxCode = gymPassProduct.Type == PassType.SingleUse ? _singleUseAccessTaxCode : _membershipTaxCode,
            Type = "service"
        };

        var product = await _productService.CreateAsync(productOptions, null, cancellationToken);

        Guard.Against.Null(product, "Product", "Failed to create gym pass product.");

        var priceOptions = new PriceCreateOptions
        {
            Product = product.Id,
            Currency = productOptions.DefaultPriceData.Currency,
            UnitAmountDecimal = productOptions.DefaultPriceData.UnitAmount
        };

        var price = await _priceService.CreateAsync(priceOptions, null, cancellationToken);

        Guard.Against.Null(price, "Price", "Failed to create gym pass product.");

        gymPassProduct.IsCreatedOnStripe = true;

        await _context.SaveChangesAsync();
    }
    
    
}