using FitPass.Domain.Entities;
using FitPass.Domain.ValueObjects;

namespace FitPass.Application.Common.Interfaces;

public interface IPaymentService
{
    Task<PaymentResult> ChargeCustomerWithPaymentIntentAsync(string customerId, Money amount, CancellationToken cancellationToken);
}
