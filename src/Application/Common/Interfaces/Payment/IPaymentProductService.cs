using FitPass.Domain.Entities;

namespace FitPass.Application.Common.Interfaces.Payment;

public interface IPaymentProductService
{
    Task CreateProduct(GymPassProduct gymPassProduct);
}
