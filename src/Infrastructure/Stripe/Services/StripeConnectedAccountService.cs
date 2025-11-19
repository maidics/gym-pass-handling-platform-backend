using FitPass.Application.Common.Interfaces.Payment;
using FitPass.Application.Common.Models;
using FitPass.Domain.Entities.Payment;
using Microsoft.Extensions.Logging;

using Stripe;

namespace FitPass.Infrastructure.Stripe.Services;

public class StripeConnectedAccountService : IPaymentTenantService
{
    private readonly ILogger<StripeConnectedAccountService> _logger;
    private readonly AccountService _accountService;
    private readonly AccountLinkService _accountLinkService;

    public StripeConnectedAccountService(
        ILogger<StripeConnectedAccountService> logger, 
        AccountService accountService, 
        AccountLinkService accountLinkService)
    {
        _logger = logger;
        _accountService = accountService;
        _accountLinkService = accountLinkService;
    }

    public async Task<Result<string>> CreateTenantAccount(
        string gymId,
        string email, 
        string businessName,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var accountOptions = new AccountCreateOptions
            {
                Type = "express", //which account do I need? standard, express? others?
                Email = email,
                BusinessType = "company", //ask for this via parameters too?
                Company = new AccountCompanyOptions
                {
                    Name = businessName
                },
                Capabilities = new AccountCapabilitiesOptions
                {
                    CardPayments = new AccountCapabilitiesCardPaymentsOptions
                    {
                        Requested = true
                    },
                    Transfers = new AccountCapabilitiesTransfersOptions
                    {
                        Requested = true
                    }
                },
                Metadata = new Dictionary<string, string>
                {
                    { "gym_Id", gymId }
                }
            };

            var account = await _accountService.CreateAsync(accountOptions, cancellationToken: cancellationToken);

            return Result<string>.Success(account.Id);
        } catch (StripeException ex)
        {
            ex.Log(_logger, nameof(StripeConnectedAccountService), nameof(CreateTenantAccount));

            return ex.ToApplicationResult<string>("Failed to create payment account.");
        }
    }

    public Task<Result<TenantPaymentAccountStatus>> GetAccountStatusAsync(string tenantAccountId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<Result<bool>> IsOnboardingCompleteAsync(string tenantAccountId, CancellationToken cancellationToken = default)
    {
        try
        {
            var account = await _accountService.GetAsync(tenantAccountId, cancellationToken: cancellationToken);

            bool isAccountOnboardingCompleted = account.DetailsSubmitted && account.ChargesEnabled && account.PayoutsEnabled;

            return Result<bool>.Success(isAccountOnboardingCompleted);
        } catch (StripeException ex)
        {
            ex.Log(_logger, nameof(StripeConnectedAccountService), nameof(IsOnboardingCompleteAsync));

            return ex.ToApplicationResult<bool>("Failed to retrieve wether onboarding is completed or not.");
        }
    }

    public async Task<Result<(string url, DateTime expiration)>> GenerateAccountLinkAsync(string accountId, string returnUrl, string refreshUrl, bool isOnboarding = false, CancellationToken cancellationToken = default)
    {
        try
        {
            var accountLinkOptions = new AccountLinkCreateOptions
            {
                Account = accountId,
                RefreshUrl = refreshUrl,
                ReturnUrl = returnUrl,
                Type = isOnboarding ? "account_onboarding" : "account_update",
                CollectionOptions = new AccountLinkCollectionOptionsOptions
                {
                    Fields = "eventually_due"
                },
            };

            var accountLink = await _accountLinkService.CreateAsync(accountLinkOptions, cancellationToken: cancellationToken);

            if (accountLink is null)
            {
                return Result<(string url, DateTime expiration)>
                    .Failure(["Failed to generate account link"], ResultType.ExternalServiceError);
            }

            return Result<(string url, DateTime expiration)>.Success((accountLink.Url, DateTime.UtcNow.AddMinutes(5)));
        } catch (StripeException ex)
        {
            ex.Log(_logger, nameof(StripeConnectedAccountService), nameof(GenerateAccountLinkAsync));

            return ex.ToApplicationResult<(string url, DateTime expiration)>("Failed to generate account link.");
        }
    }
}
