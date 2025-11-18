using FitPass.Application.Common.Interfaces.Payment;
using FitPass.Domain.ValueObjects;

namespace FitPass.Infrastructure.Stripe.Services;

public class StripePaymentService : IPaymentService
{
    public Task ChargeCustomerWithPaymentIntentAsync(string customerId, Money amount, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
