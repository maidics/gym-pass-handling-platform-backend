using FitPass.Application.Common.Models;
using FitPass.Domain.Entities.Payment;
using FitPass.Domain.Enums;

namespace FitPass.Application.Common.Interfaces.Payment;

public interface IPaymentTenantService
{
    Task<Result<string>> CreateTenantAccount(string gymId, string email, string businessName, CancellationToken cancellationToken = default);
    Task<Result<string>> GetOnboardingLinkAsync(string tenantAccountId, string returnUrl, string refreshUrl, CancellationToken cancellationToken = default);
    Task<Result<bool>> IsOnboardingCompleteAsync(string tenantAccountId, CancellationToken cancellationToken = default);
    Task<Result<TenantPaymentAccountStatus>> GetAccountStatusAsync(string tenantAccountId, CancellationToken cancellationToken = default);
}
