using FitPass.Application.Common.Interfaces.Payment;
using FitPass.Application.Common.Models;
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

    public async Task<Result<string>> CreateProductAsync(string name, string description, PassType type, bool isActive, string accountId)
    {
        try
        {
            var productOptions = new ProductCreateOptions
            {
                Name = name,
                Description = description,
                //Shippable = false,
                TaxCode = type == PassType.SingleUse ? _settings.TaxCodes.SingleUseAccess : _settings.TaxCodes.Membership,
                Active = isActive
            };

            var requestOptions = new RequestOptions
            {
                StripeAccount = accountId
            };

            var product = await _productService.CreateAsync(productOptions, null);

            return Result.Success(product.Id);
        } catch (StripeException ex)
        {
            ex.Log(_logger, nameof(StripeProductService), nameof(CreateProductAsync));

            return ex.ToResultFailure("Failed to create product on Stripe.");
        }
    }

    public async Task<Result> UpdateProductAsync(string productId, bool? isActive = null, string? name = default, string? description = default)
    {
        try
        {
            var options = new ProductUpdateOptions();

            if (!string.IsNullOrEmpty(name))
            {
                options.Name = name;
            }

            if (!string.IsNullOrEmpty(description))
            {
                options.Description = description;
            }

            if (isActive is not null)
            {
                options.Active = isActive;
            }

            await _productService.UpdateAsync(productId, options);

            return Result.Success();
        } catch (StripeException ex)
        {
            ex.Log(_logger, nameof(StripeProductService), nameof(UpdateProductAsync));

            return ex.ToResultFailure("Failed to update active status of Stripe product.");
        }
    }

    public async Task<Result> DeleteProductAsync(string productId)
    {
        try
        {
            await _productService.DeleteAsync(productId);

            return Result.Success();
        } catch(StripeException ex)
        {
            ex.Log(_logger, nameof(StripeProductService), nameof(DeleteProductAsync));

            return ex.ToResultFailure("Failed to delete product on Stripe.");
        }
    }
}
