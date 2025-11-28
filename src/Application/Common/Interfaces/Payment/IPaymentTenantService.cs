using FitPass.Application.Common.Models;
using FitPass.Domain.Enums;

namespace FitPass.Application.Common.Interfaces.Payment;

public interface IPaymentTenantService
{
    Task<Result<string>> CreateTenantAccount(string gymId, string email, string businessName, CancellationToken cancellationToken = default);
    Task<Result<(string url, DateTimeOffset expiration)>> GenerateAccountLinkAsync(string accountId, bool isOnboarding = false, CancellationToken cancellationToken = default);
    Task<Result<bool>> IsOnboardingCompleteAsync(string tenantAccountId, CancellationToken cancellationToken = default);
    Task<Result> UpdateTenantPaymentAccountPayoutIntervalAsync(string tenantAccountId, TimeIntervals interval, int? monhtlyAnchor, DayOfWeek? weeklyAnchor, int? delayDays);
    Task<Result<string>> GenerateLoginLinkAsync(string accountId, CancellationToken cancellationToken = default);
}
