using FitPass.Application.Common.Interfaces.Payment;
using FitPass.Application.Common.Models;
using FitPass.Application.Payments.Commands;
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

    public Task<Result> ProcessAsync(string json, string signature, CancellationToken cancellationToken = default)
    {
        var stripeEvent = EventUtility.ConstructEvent(json, signature, _webhookSecret);

        return stripeEvent.Type switch
        {
            EventTypes.AccountUpdated => HandleAccountUpdated(stripeEvent),
            EventTypes.PaymentIntentSucceeded => HandlePaymentIntentSucceeded(stripeEvent),
            _ => HandleUnhandled(stripeEvent)
        };
    }

    private Task<Result> HandleUnhandled(Event stripeEvent)
    {
        _logger.LogError("Unhandled Stripe Webhook event: {StripeEvent}.", stripeEvent);

        return Task.FromResult(Result.Failure("Unhandled payment provider webhook.", [], ResultTypes.InternalError));
    }

    private Result<T> GetEventData<T>(Event stripeEvent)
    {
        if (stripeEvent.Data.Object is not T instance)
        {
            _logger.LogError("{T} data is null in webhook StripeEvent {StripeEvent}", typeof(T), stripeEvent);

            return Result.BusinessRuleViolation();
        }

        return Result.Success(instance);
    }

    private async Task<Result> HandleAccountUpdated(Event stripeEvent)
    {
        var result = GetEventData<Account>(stripeEvent);

        if (!result.Succeeded)
        {
            return result;
        }

        var account = result.Value;

        return await _sender.Send(new UpdateTenantPaymentProfileAccountStatusCommand(
            TenantAccountId: account.Id,
            DetailsSubmitted: account.DetailsSubmitted,
            ChargesEnabled: account.ChargesEnabled,
            PayoutsEnabled: account.PayoutsEnabled,
            RequirementsDue: account.Requirements?.CurrentlyDue?.ToList() ?? [],
            RequirementsEventuallyDue: account.Requirements?.EventuallyDue?.ToList() ?? []
        ));
    }

    private async Task<Result> HandlePaymentIntentSucceeded(Event stripeEvent)
    {
        var result = GetEventData<PaymentIntent>(stripeEvent);

        if (!result.Succeeded)
        {
            return result;
        }

        var intent = result.Value;
        var userId = intent.Metadata.GetValueOrDefault("UserId");
        var gymId = intent.Metadata.GetValueOrDefault("GymId");
        var gymPassProductId = intent.Metadata.GetValueOrDefault("GymPassProductId");

        if (userId is null || gymId is null || gymPassProductId is null)
        {
            _logger.LogCritical("Metadata (UserId, GymId, GymPassProduct) is missing from payment intent.");
            //TODO: handle this case somehow? notify dev?
            return Result.BusinessRuleViolation("Metadata is missing from payment intent.");
        }

        return await _sender.Send(new FulFillGymPassProductPaymentCommand(userId, gymId, gymPassProductId));
    }
}
