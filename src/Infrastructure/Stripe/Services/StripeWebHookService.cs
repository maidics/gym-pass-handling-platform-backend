using FitPass.Application.Common.Interfaces.Payment;
using FitPass.Application.Common.Models;
using FitPass.Application.TenantPaymentProfiles.Commands;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Stripe;

namespace FitPass.Infrastructure.Stripe.Services;

public class StripeWebHookService : IPaymentWebhookService
{
    private readonly ILogger<StripeWebHookService> _logger;
    private readonly ISender _sender;
    private readonly string _webhookSecret;

    public StripeWebHookService(
        ILogger<StripeWebHookService> logger,
        ISender sender,
        IConfiguration configuration)
    {
        _logger = logger;
        _sender = sender;
        _webhookSecret = configuration["Stripe:WebHookSecret"] ?? throw new InvalidOperationException("Stripe webhook secret is not configured");
    }

    public Task ProcessAsync(string json, string signature, CancellationToken cancellationToken = default)
    {
        var stripeEvent = EventUtility.ConstructEvent(json, signature, _webhookSecret);

        return stripeEvent.Type switch
        {
            EventTypes.AccountUpdated => HandleAccountUpdated(stripeEvent),
            _ => HandleUnhandled(stripeEvent)
        };
    }

    private async Task HandleAccountUpdated(Event stripeEvent)
    {
        if (stripeEvent.Data.Object is not Account account)
        {
            _logger.LogError("Account data is null in webhook event {StripeEvent}.", stripeEvent);

            throw new ArgumentNullException(nameof(account));
        }

        await _sender.Send(new UpdateTenantPaymentProfileAccountStatusCommand(
            TenantAccountId: account.Id,
            DetailsSubmitted: account.DetailsSubmitted,
            ChargesEnabled: account.ChargesEnabled,
            PayoutsEnabled: account.PayoutsEnabled,
            RequirementsDue: account.Requirements?.CurrentlyDue?.ToList() ?? [],
            RequirementsEventuallyDue: account.Requirements?.EventuallyDue?.ToList() ?? []
        ));
    }

    private Task<Result> HandleUnhandled(Event stripeEvent)
    {
        _logger.LogError("Unhandled Stripe Webhook event: {StripeEvent}.", stripeEvent);

        return Task.FromResult(Result.Failure(["Unhandled payment provider webhook."], ResultType.InternalError));
    }
}
