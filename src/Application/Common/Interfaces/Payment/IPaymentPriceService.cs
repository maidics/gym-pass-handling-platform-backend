using FitPass.Application.Common.Models;
using FitPass.Domain.ValueObjects;

namespace FitPass.Application.Common.Interfaces.Payment;

public interface IPaymentPriceService
{
    Task<Result<string>> CreatePriceAsync(string productId, Money priceMoney);
    Task<Result<string>> UpdatePriceAsync(string priceId, string productId, Money newPrice);
    Task<Result> SetActiveFlagAsync(string priceId, bool isActive);
}
