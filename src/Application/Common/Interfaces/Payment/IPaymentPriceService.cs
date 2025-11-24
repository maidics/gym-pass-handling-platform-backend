using FitPass.Application.Common.Models;
using FitPass.Domain.ValueObjects;

namespace FitPass.Application.Common.Interfaces.Payment;

public interface IPaymentPriceService
{
    Task<Result<string>> CreatePrice(string productId, Money priceMoney);
}
