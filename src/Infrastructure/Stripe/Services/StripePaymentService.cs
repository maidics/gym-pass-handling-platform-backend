using FitPass.Application.Common.Interfaces;
using FitPass.Domain.Entities;
using FitPass.Domain.ValueObjects;

namespace FitPass.Infrastructure.Stripe.Services;

public class StripePaymentService : IPaymentService
{
    public Task<PaymentResult> ChargeCustomerWithPaymentIntentAsync(string customerId, Money amount, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
