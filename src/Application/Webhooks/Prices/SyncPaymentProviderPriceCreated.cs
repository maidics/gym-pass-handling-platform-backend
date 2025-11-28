using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Interfaces.Payment;
using FitPass.Application.Common.Models;
using FitPass.Application.Users.Queries;

namespace FitPass.Application.Webhooks.Prices;

//webhook only
public record SyncPaymentProviderPriceCreatedCommand(
    string PaymentPriceId,
    string PaymentAccountId
) : IRequest<Result>;

public class SyncPaymentProviderPriceCreatedCommandHandler : IRequestHandler<SyncPaymentProviderPriceCreatedCommand, Result>
{
    private readonly IPaymentPriceService _priceService;
    private readonly IClientNotificationSender _notificationSender;
    private readonly ISender _sender;

    public SyncPaymentProviderPriceCreatedCommandHandler(
        IPaymentPriceService priceService,
        IClientNotificationSender notificationSender,
        ISender sender)
    {
        _priceService = priceService;
        _notificationSender = notificationSender;
        _sender = sender;
    }

    public async Task<Result> Handle(SyncPaymentProviderPriceCreatedCommand command, CancellationToken cancellationToken)
    {
        var result = await _priceService.UpdateActiveStatusAsync(command.PaymentPriceId, false);

        var gymAdminIds = await _sender.Send(new GetAllGymAdminIdsByTenantPaymentAccountIdQuery(command.PaymentAccountId));

        var message = "Please infer from using Stripe to create prices. Use the app's adming dashboard to create GymPassProducts - this also creates the connected price. Prices created on Stripe are automatically set to inactive";

        var notification = ClientNotification.Create(message, ClientNotificationType.PaymentProviderPriceSynced);

        await _notificationSender.SendAsync(gymAdminIds, notification);

        return result;
    }
}