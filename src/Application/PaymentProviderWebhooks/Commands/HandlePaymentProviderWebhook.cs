using FitPass.Application.Common.Extensions;
using FitPass.Application.Common.Interfaces.Payment;
using FitPass.Application.Common.Models;
using Microsoft.Extensions.Logging;

namespace FitPass.Application.PaymentProviderWebhooks.Commands;

public record HandlePaymentProviderWebhookCommand(
    string Json,
    string SignatureHeader
) : IRequest<Result>;

public class HandlePaymentProviderWebhookCommandValidator : AbstractValidator<HandlePaymentProviderWebhookCommand>
{
    public HandlePaymentProviderWebhookCommandValidator()
    {
        RuleFor(v => v.Json).NotEmptyLocalized(nameof(HandlePaymentProviderWebhookCommand.Json));

        RuleFor(v => v.Json).NotEmptyLocalized(nameof(HandlePaymentProviderWebhookCommand.SignatureHeader));
    }
}

public class HandlePaymentProviderWebhookCommandHandler : IRequestHandler<HandlePaymentProviderWebhookCommand, Result>
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

    public async Task<Result> Handle(HandlePaymentProviderWebhookCommand command, CancellationToken cancellationToken)
    {
        return await _paymentWebhookService.ProcessAsync(command.Json, command.SignatureHeader);
    }
}