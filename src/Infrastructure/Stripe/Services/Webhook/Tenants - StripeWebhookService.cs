using FitPass.Application.Common.Models;
using FitPass.Application.TenantPaymentProfiles.Commands;
using FitPass.Application.Users.Queries;
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
            RequirementsDue: account.Requirements?.CurrentlyDue?.ToList() ?? [],
            RequirementsEventuallyDue: account.Requirements?.EventuallyDue?.ToList() ?? []
        ));

        var gymAdminIds = await _sender.Send(new GetAllGymAdminIdsByTenantPaymentAccountIdQuery(account.Id));

        var notification = ClientNotification.Create("Your Stripe payment account has been updated.", ClientNotificationType.Default);

        await _notificationSender.SendAsync(gymAdminIds, notification);

        return Result.Success();
    }
}
