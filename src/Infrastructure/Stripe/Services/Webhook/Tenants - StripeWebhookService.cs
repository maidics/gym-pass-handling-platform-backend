using FitPass.Application.Common.Models;
using FitPass.Application.Users.Queries;
using FitPass.Application.Webhooks;
using Stripe;

namespace FitPass.Infrastructure.Stripe.Services.Webhook;

public partial class StripeWebhookService
{
    private async Task<Result> HandleAccountUpdated(Event stripeEvent)
    {
        var result = GetEventData<Account>(stripeEvent);

        if (!result.Succeeded)
        {
            return result;
        }

        var account = result.Value;

        await _sender.Send(new UpdateTenantPaymentProfileAccountStatusCommand(
            TenantAccountId: account.Id,
            DetailsSubmitted: account.DetailsSubmitted,
            ChargesEnabled: account.ChargesEnabled,
            PayoutsEnabled: account.PayoutsEnabled,
            RequirementsDue: account.Requirements?.CurrentlyDue.ToArray() ?? [],
            RequirementsEventuallyDue: account.Requirements?.EventuallyDue?.ToArray() ?? []
        ));

        var gymAdminIds = await _sender.Send(new GetAllGymAdminIdsByTenantPaymentAccountIdQuery(account.Id));

        var notification = ClientNotification.Create("Your Stripe payment account has been updated.", ClientNotificationType.Default);

        await _notificationSender.SendAsync(gymAdminIds, notification);

        return Result.Success();
    }
}
