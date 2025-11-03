using FitPass.Application.Common.Interfaces;
using FitPass.Domain.Entities;
using FitPass.Infrastructure.Stripe;
using Microsoft.Extensions.Logging;
using Stripe;

namespace Fitpass.Infrastructure.Stripe.Services;

public class StripeCustomerService : IStripeCustomerService
{
    private readonly ILogger<StripeCustomerService> _logger;
    private readonly CustomerService _customerService;
    public StripeCustomerService(
        ILogger<StripeCustomerService> logger,
        CustomerService customerService)
    {
        _logger = logger;
        _customerService = customerService;
    }

    public async Task<string> CreateStripeCustomer(UserProfile userProfile, string email)
    {
        try
        {
            var customerOptions = new CustomerCreateOptions
            {
                Name = $"{userProfile.FirstName} {userProfile.LastName}",
                Email = email,
            };

            var customer = await _customerService.CreateAsync(customerOptions, null);

            return customer.Id;
        }
        catch (StripeException ex)
        {
            throw ex.LogAndGetApplicationException(_logger);
        }
    }

    public async Task DeleteCustomerFromStripe(string stripecustomerId)
    {
        try
        {
            await _customerService.DeleteAsync(stripecustomerId);
        }
        catch (StripeException ex)
        {
            throw ex.LogAndGetApplicationException(_logger);
        }
    }
}
