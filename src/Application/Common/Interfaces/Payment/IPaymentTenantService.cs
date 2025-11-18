using FitPass.Application.Common.Models;
using FitPass.Domain.Enums;

namespace FitPass.Application.Common.Interfaces;

public interface IPaymentTenantService
{
    Task<Result<(string tenantAccountId, string onboardingUrl, DateTime expirationTime), PaymentFailure>> CreateTenantAccountAndGetOnboardingLinkAsync(string gymId, string email, string businessName, string returnUrl, string refreshUrl, CancellationToken cancellationToken = default);
    Task<Result<string, PaymentFailure>> GetOnboardingLinkAsync(string tenantAccountId, string returnUrl, string refreshUrl, CancellationToken cancellationToken = default);
    Task<Result<bool, PaymentFailure>> IsOnboardingCompleteAsync(string tenantAccountId, CancellationToken cancellationToken = default);
}
