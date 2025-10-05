using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Models;
using FitPass.Domain.Entities;
using FitPass.Infrastructure.Stripe;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Stripe;

namespace Fitpass.Infrastructure.Stripe.Services;

public class StripeCustomerService : IStripeCustomerService
{
    private readonly StripeSettings _settings;
    private readonly ILogger<StripeCustomerService> _logger;
    private readonly CustomerService _customerService;
    private readonly IApplicationDbContext _context;

    public StripeCustomerService(IOptions<StripeSettings> options, ILogger<StripeCustomerService> logger, CustomerService customerService, IApplicationDbContext context)
    {
        _settings = options.Value;
        _logger = logger;
        _customerService = customerService;
        _context = context;
    }

    public async Task<Result> CreateCustomer(ApplicationUser user, CancellationToken cancellationToken)
    {
        try
        {
            var customerOptions = new CustomerCreateOptions
            {
                Name = $"{user.FirstName} {user.LastName}",
                Email = user.Email,
            };

            cancellationToken.ThrowIfCancellationRequested();

            var customer = await _customerService.CreateAsync(customerOptions, null, cancellationToken);

            var paymentProfile = new UserPaymentProfile
            {
                ApplicationUserId = user.Id,
                StripeCustomerId = customer.Id
            };

            await _context.UserPaymentProfiles.AddAsync(paymentProfile);

            user.UserPaymentProfileId = paymentProfile.Id;

            await _context.SaveChangesAsync();

            return Result.Success();
        }
        catch (StripeException ex)
        {
            return ex.LogAndGetResult<StripeCustomerService>(_logger);
        }
    }

    public async Task<Result> DeleteCustomer(ApplicationUser user, CancellationToken cancellationToken)
    {
        try
        {
            if (user.PaymentProfile == null || user.PaymentProfile.StripeCustomerId == null)
            {
                return Result.Failure(["User is not a Stripe customer."]);
            }

            cancellationToken.ThrowIfCancellationRequested();

            await _customerService.DeleteAsync(user.PaymentProfile.StripeCustomerId);

            user.PaymentProfile.StripeCustomerId = null;

            await _context.SaveChangesAsync();

            return Result.Success();
        } catch (StripeException ex)
        {
            return ex.LogAndGetResult<StripeCustomerService>(_logger);
        }
    }
}
