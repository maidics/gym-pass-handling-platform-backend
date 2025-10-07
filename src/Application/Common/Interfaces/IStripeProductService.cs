using FitPass.Application.Common.Models;
using FitPass.Domain.Entities;

namespace FitPass.Application.Common.Interfaces;

public interface IStripeProductService
{
    Task CreateProduct(GymPassProduct gymPassProduct);
}
