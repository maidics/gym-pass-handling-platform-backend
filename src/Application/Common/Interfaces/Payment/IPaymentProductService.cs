using FitPass.Application.Common.Models;
using FitPass.Domain.Enums;

namespace FitPass.Application.Common.Interfaces.Payment;

public interface IPaymentProductService
{
    Task<Result<string>> CreateProductAsync(string name, string description, PassType type, bool isActive, string accountId);
    Task<Result> UpdateProductAsync(string productId, string paymentAccountId, bool? isActive = null, string? name = null, string? description = null);
    Task<Result> DeleteProductAsync(string productId, string paymentAccountId);
}
