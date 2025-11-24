using FitPass.Application.Common.Interfaces.Payment;
using FitPass.Application.Common.Models;
using FitPass.Domain.Entities;
using FitPass.Domain.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Stripe;

namespace FitPass.Infrastructure.Stripe.Services;

public class StripeProductService : IPaymentProductService
{
    private readonly StripeSettings _settings;
    private readonly ILogger<StripeProductService> _logger;
    private readonly ProductService _productService;

    public StripeProductService(
        IOptions<StripeSettings> options, 
        ILogger<StripeProductService> logger, 
        ProductService productService)
    {
        _settings = options.Value;
        _logger = logger;
        _productService = productService;
    }

    public async Task<Result<string>> CreateProduct(string name, string description, PassType type)
    {
        try
        {
            var productOptions = new ProductCreateOptions
            {
                Name = name,
                Description = description,
                Shippable = false,
                TaxCode = type == PassType.SingleUse ? _settings.TaxCodeSettings.SingleUseAccess : _settings.TaxCodeSettings.Membership,
                Type = "service"
            };

            var product = await _productService.CreateAsync(productOptions, null);

            return Result.Success(product.Id);
        } catch (StripeException ex)
        {
            ex.Log(_logger, nameof(StripeProductService), nameof(CreateProduct));

            return ex.ToResultFailure("Failed to create product on Stripe.");
        }
    }
}