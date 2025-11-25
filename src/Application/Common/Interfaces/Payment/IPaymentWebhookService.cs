using FitPass.Application.Common.Models;

namespace FitPass.Application.Common.Interfaces.Payment;

public interface IPaymentWebhookService
{
    Task<Result> ProcessAsync(string json, string signature, CancellationToken cancellationToken = default);
}
