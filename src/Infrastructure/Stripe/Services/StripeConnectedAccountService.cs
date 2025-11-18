using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Models;
using FitPass.Domain.Enums;
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

    public async Task<Result<string, PaymentProviderResult>> CreateTenantAccount(
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

            return Result<string, PaymentProviderResult>.Success(account.Id, PaymentProviderResult.Success);
        } catch (StripeException ex)
        {
            ex.Log(_logger, nameof(StripeConnectedAccountService), nameof(CreateTenantAccount));

            return ex.ToApplicationResult<string>();
        }
    }

    public async Task<Result<string, PaymentProviderResult>> GetOnboardingLinkAsync(
        string tenantAccountId, 
        string returnUrl, 
        string refreshUrl, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var accountLink = await GenerateAccountLinkAsync(tenantAccountId, returnUrl, refreshUrl, cancellationToken);

            return Result<string, PaymentProviderResult>.Success(accountLink, PaymentProviderResult.Success);
        } catch (StripeException ex)
        {
            ex.Log(_logger, nameof(StripeConnectedAccountService), nameof(GetOnboardingLinkAsync));

            return ex.ToApplicationResult<string>();
        }
    }

    public async Task<Result<bool, PaymentProviderResult>> IsOnboardingCompleteAsync(string tenantAccountId, CancellationToken cancellationToken = default)
    {
        try
        {
            var account = await _accountService.GetAsync(tenantAccountId, cancellationToken: cancellationToken);

            bool isAccountOnboardingCompleted = account.DetailsSubmitted && account.ChargesEnabled && account.PayoutsEnabled;

            return Result<bool, PaymentProviderResult>.Success(isAccountOnboardingCompleted, PaymentProviderResult.Success);
        } catch (StripeException ex)
        {
            ex.Log(_logger, nameof(StripeConnectedAccountService), nameof(IsOnboardingCompleteAsync));

            return ex.ToApplicationResult<bool>();
        }
    }

    private async Task<string> GenerateAccountLinkAsync(string accountId, string returnUrl, string refreshUrl, CancellationToken cancellationToken = default)
    {
        var accountLinkOptions = new AccountLinkCreateOptions 
        {
            Account = accountId,
            RefreshUrl = refreshUrl,
            ReturnUrl = returnUrl,
            Type = "account_onboarding", //??
            CollectionOptions = new AccountLinkCollectionOptionsOptions
            {
                Fields = "eventually_due"
            }
        };

        var accountLink = await _accountLinkService.CreateAsync(accountLinkOptions, cancellationToken: cancellationToken);

        return accountLink.Url;
    }
}
