using FitPass.Application.Common.Models;
using FitPass.Domain.Entities;

namespace FitPass.Application.Common.Interfaces;

public interface IStripeProductService
{
    Task<Result> CreateProduct(GymPassProduct gymPassProduct, CancellationToken cancellationToken);
}
