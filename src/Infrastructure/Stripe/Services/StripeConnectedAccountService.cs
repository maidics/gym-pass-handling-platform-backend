using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Interfaces.Payment;
using FitPass.Application.Common.Models;
using FitPass.Application.Common.Resources;
using FitPass.Application.Common.Settings;
using FitPass.Application.TenantPaymentProfiles.DTOs;
using FitPass.Domain.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Stripe;

namespace FitPass.Infrastructure.Stripe.Services;

public class StripeConnectedAccountService : IPaymentTenantService
{
    private readonly ILogger<StripeConnectedAccountService> _logger;
    private readonly AccountService _accountService;
    private readonly AccountLinkService _accountLinkService;
    private readonly AccountLoginLinkService _loginLinkService;
    private readonly StripeSettings _stripeSettings;
    private readonly ClientAppSettings _clientAppSettings;
    private readonly ILocalizer _localizer;

    public StripeConnectedAccountService(
        ILogger<StripeConnectedAccountService> logger, 
        AccountService accountService, 
        AccountLoginLinkService loginLinkService,
        AccountLinkService accountLinkService,
        IOptions<StripeSettings> stripeOptions,
        IOptions<ClientAppSettings> clientAppOptions,
        ILocalizer localizer)
    {
        _logger = logger;
        _accountService = accountService;
        _loginLinkService = loginLinkService;
        _accountLinkService = accountLinkService;
        _stripeSettings = stripeOptions.Value;
        _clientAppSettings = clientAppOptions.Value;
        _localizer = localizer;
    }

    public async Task<Result<string>> CreateTenantAccount(
        string gymId,
        string email, 
        string businessName,
        CancellationToken cancellationToken)
    {
        try
        {
            var accountOptions = new AccountCreateOptions
            {
                Type = "express", //which account do I need? standard, express? others?
                Email = email,
                BusinessType = "company",
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
                },
                BusinessProfile = new AccountBusinessProfileOptions()
                {
                    Mcc = "7997"
                }
            };

            var account = await _accountService.CreateAsync(accountOptions, cancellationToken: cancellationToken);

            return Result<string>.Success(account.Id);
        } catch (StripeException ex)
        {
            ex.Log(_logger, nameof(StripeConnectedAccountService), nameof(CreateTenantAccount));

            return ex.ToResultFailure(_localizer.GetExternalServiceNotAvailable("Stripe"));
        }
    }

    public async Task<Result<bool>> IsOnboardingCompleteAsync(string tenantAccountId, CancellationToken cancellationToken)
    {
        try
        {
            var account = await _accountService.GetAsync(tenantAccountId, cancellationToken: cancellationToken);

            if (account is null)
            {
                return Result.NotFound(_localizer.Get(nameof(SharedResource.RequiresStripeAccount)));
            }
            
            return Result.Success(account.DetailsSubmitted);
        } catch (StripeException ex)
        {
            ex.Log(_logger, nameof(StripeConnectedAccountService), nameof(IsOnboardingCompleteAsync));

            return ex.ToResultFailure(_localizer.GetExternalServiceNotAvailable("Stripe"));
        }
    }

    public async Task<Result<PaymentProviderLinkDto>> GenerateAccountLinkAsync(
        string accountId, string gymId, bool isOnboarding = false, bool fallbackToOnboarding = false, CancellationToken cancellationToken = default)
    {
        try
        {
            var accountLinkOptions = new AccountLinkCreateOptions
            {
                Account = accountId,
                RefreshUrl = _stripeSettings.GetAccountLinkRefreshPath(_clientAppSettings.BaseUrl, gymId),
                ReturnUrl = _stripeSettings.GetAccountLinkReturnPath(_clientAppSettings.BaseUrl, gymId),
                Type = isOnboarding ? "account_onboarding" : "account_update",
                CollectionOptions = new AccountLinkCollectionOptionsOptions
                {
                    Fields = "eventually_due"
                },
            };

            var accountLink = await _accountLinkService.CreateAsync(accountLinkOptions, cancellationToken: cancellationToken);

            if (accountLink is null)
            {
                return Result.ExternalServiceUnavailable(_localizer.GetExternalServiceNotAvailable("Stripe"));
            }

            return Result.Success(new PaymentProviderLinkDto(accountLink.Url, PaymentProviderLinkType.AccountLink));
        } catch (StripeException ex)
        {
            var msg = ex.StripeError?.Message ?? ex.Message;

            if (!isOnboarding &&
                fallbackToOnboarding &&
                msg.Contains("Valid types for this account are", StringComparison.OrdinalIgnoreCase) &&
                msg.Contains("account_onboarding", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    return await GenerateAccountLinkAsync(accountId, gymId, true, false, cancellationToken);
                }
                catch (StripeException fallbackEx)
                {
                    fallbackEx.Log(_logger, nameof(StripeConnectedAccountService), nameof(GenerateAccountLinkAsync));
                    return fallbackEx.ToResultFailure(_localizer.GetExternalServiceNotAvailable("Stripe"));
                }
            }
            
            ex.Log(_logger, nameof(StripeConnectedAccountService), nameof(GenerateAccountLinkAsync));

            return ex.ToResultFailure(_localizer.GetExternalServiceNotAvailable("Stripe"));
        }
    }

    public async Task<Result<PaymentProviderLinkDto>> GenerateLoginLinkAsync(string accountId, CancellationToken cancellationToken)
    {
        try
        {
            //var options = new AccountLoginLinkCreateOptions();

            var loginLink = await _loginLinkService.CreateAsync(accountId, cancellationToken: cancellationToken);

            return Result.Success(new PaymentProviderLinkDto(loginLink.Url, PaymentProviderLinkType.LoginLink));
        } catch (StripeException ex)
        {
            ex.Log(_logger, nameof(StripeConnectedAccountService), nameof(GenerateLoginLinkAsync));

            return ex.ToResultFailure(_localizer.GetExternalServiceNotAvailable("Stripe"));
        }
    }
}
