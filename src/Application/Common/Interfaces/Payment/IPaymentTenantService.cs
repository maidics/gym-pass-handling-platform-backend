using FitPass.Application.Common.Models;
using FitPass.Domain.Entities.Payment;

namespace FitPass.Application.Common.Interfaces.Payment;

public interface IPaymentTenantService
{
    Task<Result<string>> CreateTenantAccount(string gymId, string email, string businessName, CancellationToken cancellationToken = default);
    Task<Result<(string url, DateTime expiration)>> GenerateAccountLinkAsync(string accountId, bool isOnboarding = false, CancellationToken cancellationToken = default);
    Task<Result<bool>> IsOnboardingCompleteAsync(string tenantAccountId, CancellationToken cancellationToken = default);
    Task<Result<TenantPaymentAccountStatus>> GetAccountStatusAsync(string tenantAccountId, CancellationToken cancellationToken = default);
}
