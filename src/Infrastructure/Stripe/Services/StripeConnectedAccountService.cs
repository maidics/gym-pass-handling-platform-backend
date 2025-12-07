using FitPass.Application.Common.Interfaces.Payment;
using FitPass.Application.Common.Models;
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
    private readonly StripeAccountLinkSettings _stripeAccountLinkSettings;
    private readonly TimeProvider _timeProvider;

    public StripeConnectedAccountService(
        ILogger<StripeConnectedAccountService> logger, 
        AccountService accountService, 
        AccountLoginLinkService loginLinkService,
        AccountLinkService accountLinkService,
        IOptions<StripeSettings> options,
        TimeProvider timeProvider)
    {
        _logger = logger;
        _accountService = accountService;
        _loginLinkService = loginLinkService;
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

    public async Task<Result> UpdateTenantPaymentAccountPayoutIntervalAsync(string tenantAccountId, TimeIntervals interval, int? monthlyAnchor, DayOfWeek? weeklyAnchor, int? delayDays)
    {
        try
        {
            var options = new AccountUpdateOptions
            {
                Settings = new AccountSettingsOptions
                {
                    Payouts = new AccountSettingsPayoutsOptions
                    {
                        Schedule = new AccountSettingsPayoutsScheduleOptions
                        {
                            Interval = interval.ToString().ToLowerInvariant()
                        }
                    }
                }
            };

            if (interval == TimeIntervals.Daily)
            {
                Guard.Against.Null(delayDays);

                options.Settings.Payouts.Schedule.DelayDays = delayDays;
            } else if (interval == TimeIntervals.Weekly)
            {
                Guard.Against.Null(weeklyAnchor);

                options.Settings.Payouts.Schedule.WeeklyAnchor = weeklyAnchor.ToString()!.ToLowerInvariant();
            } else if (interval == TimeIntervals.Monthly)
            {
                Guard.Against.Null(monthlyAnchor);

                options.Settings.Payouts.Schedule.MonthlyAnchor = monthlyAnchor;
            }

            var account = await _accountService.UpdateAsync(tenantAccountId, options);

            return Result.Success();
        } catch (StripeException ex)
        {
            ex.Log(_logger, nameof(StripeConnectedAccountService), nameof(UpdateTenantPaymentAccountPayoutIntervalAsync));

            return ex.ToResultFailure("Failed to update payment account's payout interval.");
        }
    }

    public async Task<Result<string>> GenerateLoginLinkAsync(string accountId, CancellationToken cancellationToken = default)
    {
        try
        {
            var options = new AccountLoginLinkCreateOptions();

            var link = await _loginLinkService.CreateAsync(accountId);

            return Result.Success(link.Url);
        } catch (StripeException ex)
        {
            ex.Log(_logger, nameof(StripeConnectedAccountService), nameof(GenerateLoginLinkAsync));

            return ex.ToResultFailure("Failed to generate Stripe login link");
        }
    }
}
