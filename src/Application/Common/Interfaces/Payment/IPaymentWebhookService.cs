namespace FitPass.Application.Common.Interfaces.Payment;

public interface IPaymentWebhookService
{
    Task ProcessAsync(string json, string signature, CancellationToken cancellationToken = default);
}
