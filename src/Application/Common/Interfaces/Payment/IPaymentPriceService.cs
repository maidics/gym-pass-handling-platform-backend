using FitPass.Application.Common.Models;
using FitPass.Domain.ValueObjects;

namespace FitPass.Application.Common.Interfaces.Payment;

public interface IPaymentPriceService
{
    Task<Result<string>> CreatePriceAsync(string productId, Money priceMoney, bool isActive, string paymentAccountId);
    Task<Result<string>> UpdatePriceAsync(string priceId, string productId, Money newPrice, bool isActive, string paymentAccountId);
    Task<Result> UpdateActiveStatusAsync(string priceId, string paymentAccountId, bool isActive);
    Result ValidateMoney(Money money);
}
