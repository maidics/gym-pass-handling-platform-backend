using FitPass.Application.Common.Interfaces.Payment;
using FitPass.Application.Common.Models;

namespace FitPass.Application.PaymentProviderWebhooks.Commands;

public record HandlePaymentProviderWebhookCommand(
    string Json,
    string SignatureHeader
) : IRequest<Result>;

public class HandlePaymentProviderWebhookCommandValidator : AbstractValidator<HandlePaymentProviderWebhookCommand>
{
    public HandlePaymentProviderWebhookCommandValidator()
    {
        RuleFor(v => v.Json).NotEmpty();

        RuleFor(v => v.Json).NotEmpty();
    }
}

public class HandlePaymentProviderWebhookCommandHandler : IRequestHandler<HandlePaymentProviderWebhookCommand, Result>
{
    private readonly IPaymentWebhookService _paymentWebhookService;

    public HandlePaymentProviderWebhookCommandHandler(
        IPaymentWebhookService paymentWebhookService)
    {
        _paymentWebhookService = paymentWebhookService;
    }

    public async Task<Result> Handle(HandlePaymentProviderWebhookCommand command, CancellationToken cancellationToken)
    {
        return await _paymentWebhookService.ProcessAsync(command.Json, command.SignatureHeader);
    }
}
