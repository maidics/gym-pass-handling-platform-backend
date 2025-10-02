using FitPass.Domain.Entities;

namespace FitPass.Application.Common.Interfaces;

public interface IPaymentService
{
    Task CreateProduct(GymPassProduct gymPassProduct, CancellationToken cancellationToken);
    Task CreateCustomer(ApplicationUser user, CancellationToken cancellationToken);
}
