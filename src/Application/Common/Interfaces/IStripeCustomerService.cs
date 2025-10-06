using FitPass.Application.Common.Models;
using FitPass.Domain.Entities;

namespace FitPass.Application.Common.Interfaces;

public interface IStripeCustomerService
{
    Task<Result> CreateCustomer(ApplicationUser user);
    Task<Result> DeleteCustomer(ApplicationUser user);
}
