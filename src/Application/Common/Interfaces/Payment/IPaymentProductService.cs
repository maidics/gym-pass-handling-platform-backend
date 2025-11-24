using FitPass.Application.Common.Models;
using FitPass.Domain.Entities;
using FitPass.Domain.Enums;

namespace FitPass.Application.Common.Interfaces.Payment;

public interface IPaymentProductService
{
    Task<Result<string>> CreateProductAsync(string name, string description, PassType type);
}
