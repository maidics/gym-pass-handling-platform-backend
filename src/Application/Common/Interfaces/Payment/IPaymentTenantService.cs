using FitPass.Application.Common.Models;
using FitPass.Application.TenantPaymentProfiles.DTOs;
using FitPass.Domain.Enums;

namespace FitPass.Application.Common.Interfaces.Payment;

public interface IPaymentTenantService
{
    Task<Result<string>> CreateTenantAccount(string gymId, string email, string businessName, CancellationToken cancellationToken = default);
    //Sends user to Stripe hosted onboarding or update flows
    Task<Result<PaymentProviderLinkDto>> GenerateAccountLinkAsync(string accountId, string gymId, bool isOnboarding = false, CancellationToken cancellationToken = default);
    Task<Result<bool>> IsOnboardingCompleteAsync(string tenantAccountId, CancellationToken cancellationToken = default);
    Task<Result> UpdateTenantPaymentAccountPayoutIntervalAsync(string tenantAccountId, TimeIntervals interval, int? monthlyAnchor, DayOfWeek? weeklyAnchor, int? delayDays);
    //Sends user to Stripe's Dashboard for express accounts
    Task<Result<PaymentProviderLinkDto>> GenerateLoginLinkAsync(string accountId, CancellationToken cancellationToken = default);
}
