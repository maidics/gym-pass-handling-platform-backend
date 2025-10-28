using FitPass.Domain.Entities;

namespace FitPass.Application.Common.Interfaces;

public interface IStripeCustomerService
{
    Task<string> CreateStripeCustomer(UserProfile userProfile, string email);
    Task DeleteCustomerFromStripe(string applicationUserId);
}
