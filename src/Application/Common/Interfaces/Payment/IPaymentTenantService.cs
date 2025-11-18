using FitPass.Application.Common.Models;
using FitPass.Domain.Entities.Payment;
using FitPass.Domain.Enums;

namespace FitPass.Application.Common.Interfaces;

public interface IPaymentTenantService
{
    Task<Result<string, PaymentProviderResult>> CreateTenantAccount(string gymId, string email, string businessName, CancellationToken cancellationToken = default);
    Task<Result<string, PaymentProviderResult>> GetOnboardingLinkAsync(string tenantAccountId, string returnUrl, string refreshUrl, CancellationToken cancellationToken = default);
    Task<Result<bool, PaymentProviderResult>> IsOnboardingCompleteAsync(string tenantAccountId, CancellationToken cancellationToken = default);
    Task<Result<TenantPaymentAccountStatus, PaymentProviderResult>> GetAccountStatusAsync(string tenantAccountId, CancellationToken cancellationToken = default);
}
