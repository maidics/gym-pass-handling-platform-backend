using FitPass.Application.Common.Models;
using FitPass.Domain.Entities;

namespace FitPass.Application.Common.Interfaces;

public interface IStripeCustomerService
{
    Task CreateCustomer(ApplicationUser user);
    Task DeleteCustomer(ApplicationUser user);
    Task CreateCustomer(NonRegisteredUser user);
}
