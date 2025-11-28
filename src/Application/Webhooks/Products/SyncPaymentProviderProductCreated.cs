using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Interfaces.Payment;
using FitPass.Application.Common.Models;
using FitPass.Application.Users.Queries;
using Microsoft.Extensions.Logging;

namespace FitPass.Application.Webhooks.Products;

//webhook only
//if it is created through Stripe then immediately deleted because business specific properties cannot be set
//TODO: solve this later ^
public record SyncPaymentProviderProductCreatedCommand(
    string PaymentProductId,
    string PaymentAccountId
) : IRequest<Result>;

public class SyncPaymentProviderProductCreatedCommandHandler : IRequestHandler<SyncPaymentProviderProductCreatedCommand, Result>
{
    private readonly IPaymentProductService _productService;
    private readonly ISender _sender;
    private readonly IClientNotificationSender _notificationSender;
    private readonly ILogger<SyncPaymentProviderProductCreatedCommandHandler> _logger;

    public SyncPaymentProviderProductCreatedCommandHandler(
        IPaymentProductService productService,
        ISender sender,
        IClientNotificationSender notificationSender,
        ILogger<SyncPaymentProviderProductCreatedCommandHandler> logger)
    {
        _productService = productService;
        _sender = sender;
        _notificationSender = notificationSender;
        _logger = logger;
    }

    public async Task<Result> Handle(SyncPaymentProviderProductCreatedCommand command, CancellationToken cancellationToken)
    {
        Result? result = null;

        try
        {
            result = await _productService.DeleteProductAsync(command.PaymentProductId);

            if (!result.Succeeded)
            {
                _logger.LogError(
                    "Failed to delete newly created product on Stripe. Connected account id: {ConnectedAccountId}, product id: {ProductId}",
                    command.PaymentAccountId,
                    command.PaymentProductId);
            }
        } catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to delete newly created product on Stripe. Connected account id: {ConnectedAccountId}, product id: {ProductId}",
                command.PaymentAccountId,
                command.PaymentProductId);
        }

        var gymAdminIds = await _sender.Send(new GetAllGymAdminIdsByTenantPaymentAccountIdQuery(command.PaymentAccountId));

        string message = "Please infer from creating the product on Stripe's website and use the app's own admin dashboard for this action. Products created through Stripe are immediately deleted.";
        var notification = ClientNotification.Create(message, ClientNotificationType.GymPassProductSynced);

        await _notificationSender.SendAsync(gymAdminIds, notification);

        return result!;
    }
}