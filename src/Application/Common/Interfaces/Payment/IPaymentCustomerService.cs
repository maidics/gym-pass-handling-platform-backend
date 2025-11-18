using FitPass.Domain.Entities;

namespace FitPass.Application.Common.Interfaces.Payment;

public interface IPaymentCustomerService
{
    Task<string> CreateStripeCustomer(UserProfile userProfile, string email);
    Task DeleteCustomerFromStripe(string applicationUserId);
}
