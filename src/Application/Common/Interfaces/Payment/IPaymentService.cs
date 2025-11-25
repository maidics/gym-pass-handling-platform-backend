using FitPass.Application.Common.Models;
using FitPass.Application.Payments.DTOs;
using FitPass.Domain.ValueObjects;

namespace FitPass.Application.Common.Interfaces.Payment;

public interface IPaymentService
{
    Task<Result<string>> CreateOneTimePaymentIntent(Money money, string userId, string gymPassProductId, string tenantPaymentAccountId);
}
