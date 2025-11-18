using FitPass.Application.Common.Extensions;
using FitPass.Application.Common.Interfaces.Payment;
using Microsoft.Extensions.Logging;

namespace FitPass.Application.PaymentProviderWebhooks.Commands;

public record HandlePaymentProviderWebhookCommand(
    string Json,
    string SignatureHeader
) : IRequest;

public class HandlePaymentProviderWebhookCommandValidator : AbstractValidator<HandlePaymentProviderWebhookCommand>
{
    public HandlePaymentProviderWebhookCommandValidator()
    {
        RuleFor(v => v.Json).NotEmptyWithMessage(nameof(HandlePaymentProviderWebhookCommand.Json));

        RuleFor(v => v.Json).NotEmptyWithMessage(nameof(HandlePaymentProviderWebhookCommand.SignatureHeader));
    }
}

public class HandlePaymentProviderWebhookCommandHandler : IRequestHandler<HandlePaymentProviderWebhookCommand>
{
    private readonly IPaymentWebhookService _paymentWebhookService;
    private readonly ILogger<HandlePaymentProviderWebhookCommandHandler> _logger;

    public HandlePaymentProviderWebhookCommandHandler(
        IPaymentWebhookService paymentWebhookService,
        ILogger<HandlePaymentProviderWebhookCommandHandler> logger)
    {
        _paymentWebhookService = paymentWebhookService;
        _logger = logger;
    }

    public async Task Handle(HandlePaymentProviderWebhookCommand command, CancellationToken cancellationToken)
    {
        await _paymentWebhookService.ProcessAsync(command.Json, command.SignatureHeader);
    }
}