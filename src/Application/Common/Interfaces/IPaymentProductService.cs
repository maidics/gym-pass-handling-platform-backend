using FitPass.Domain.Entities;

namespace FitPass.Application.Common.Interfaces;

public interface IPaymentProductService
{
    Task CreateProduct(GymPassProduct gymPassProduct);
}
