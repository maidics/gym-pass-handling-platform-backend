using FitPass.Application.Common.Interfaces.Payment;
using FitPass.Application.Common.Models;
using FitPass.Domain.Entities.Payment;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Stripe;

namespace FitPass.Infrastructure.Stripe.Services;

public class StripeConnectedAccountService : IPaymentTenantService
{
    private readonly ILogger<StripeConnectedAccountService> _logger;
    private readonly AccountService _accountService;
    private readonly AccountLinkService _accountLinkService;
    private readonly StripeAccountLinkSettings _stripeAccountLinkSettings;
    private readonly TimeProvider _timeProvider;

    public StripeConnectedAccountService(
        ILogger<StripeConnectedAccountService> logger, 
        AccountService accountService, 
        AccountLinkService accountLinkService,
        IOptions<StripeSettings> options,
        TimeProvider timeProvider)
    {
        _logger = logger;
        _accountService = accountService;
        _accountLinkService = accountLinkService;
        _stripeAccountLinkSettings = options.Value.AccountLinks;
        _timeProvider = timeProvider;
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

            return ex.ToResultFailure("Failed to create payment account.");
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

            return ex.ToResultFailure("Failed to retrieve wether onboarding is completed or not.");
        }
    }

    public async Task<Result<(string url, DateTimeOffset expiration)>> GenerateAccountLinkAsync(string accountId, bool isOnboarding = false, CancellationToken cancellationToken = default)
    {
        try
        {
            var accountLinkOptions = new AccountLinkCreateOptions
            {
                Account = accountId,
                RefreshUrl = _stripeAccountLinkSettings.RefreshUrl,
                ReturnUrl = _stripeAccountLinkSettings.ReturnUrl,
                Type = isOnboarding ? "account_onboarding" : "account_update",
                CollectionOptions = new AccountLinkCollectionOptionsOptions
                {
                    Fields = "eventually_due"
                },
            };

            var accountLink = await _accountLinkService.CreateAsync(accountLinkOptions, cancellationToken: cancellationToken);

            if (accountLink is null)
            {
                return Result.ExternalServiceUnavailable("Failed to generate account link");
            }

            return Result<(string url, DateTimeOffset expiration)>.Success((accountLink.Url, _timeProvider.GetUtcNow().AddMinutes(5)));
        } catch (StripeException ex)
        {
            ex.Log(_logger, nameof(StripeConnectedAccountService), nameof(GenerateAccountLinkAsync));

            return ex.ToResultFailure("Failed to generate account link.");
        }
    }
}
