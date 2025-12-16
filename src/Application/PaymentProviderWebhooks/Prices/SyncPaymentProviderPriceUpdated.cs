using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Interfaces.Payment;
using FitPass.Application.Common.Models;
using FitPass.Application.Users.Queries;
using FitPass.Domain.Entities;

namespace FitPass.Application.Webhooks.Prices;

//webhook only
public record SyncPaymentProviderPriceUpdatedCommand(
    string PaymentPriceId,
    string PaymentAccountId
) : IRequest<Result>;

public class SyncPaymentProviderPriceUpdatedCommandHandler : IRequestHandler<SyncPaymentProviderPriceUpdatedCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IPaymentPriceService _priceService;
    private readonly IClientNotificationSender _notificationSender;
    private readonly ISender _sender;

    public SyncPaymentProviderPriceUpdatedCommandHandler(
        IApplicationDbContext context,
        IPaymentPriceService priceService,
        IClientNotificationSender notificationSender,
        ISender sender)
    {
        _context = context;
        _priceService = priceService;
        _notificationSender = notificationSender;
        _sender = sender;
    }

    //since some currencies are not supported by the backend this handler has to revert the price
    public async Task<Result> Handle(SyncPaymentProviderPriceUpdatedCommand command, CancellationToken cancellationToken)
    {
        var product = await _context.GymPassProducts
            .Include(x => x.PaymentIdentity)
            .FirstOrDefaultAsync(x => x.PaymentIdentity.PriceId == command.PaymentPriceId);

        Guard.Against.Null(product, nameof(GymPassProduct), $"Failed to find related GymPassProduct to updated '{command.PaymentPriceId}' price");

        var result = await _priceService.UpdatePriceAsync(
            product.PaymentIdentity.PriceId, 
            product.PaymentIdentity.Id, 
            product.Price, 
            product.IsActive,
            command.PaymentAccountId);

        var gymAdminIds = await _sender.Send(new GetAllGymAdminIdsByTenantPaymentAccountIdQuery(command.PaymentAccountId));

        var message = "Please infer from updating the product's price on Stripe. Use the app's admin dashboard instead. Price changes will be reverted.";

        var notification = ClientNotification.Create(message, ClientNotificationType.PaymentProviderPriceSynced);

        await _notificationSender.SendAsync(gymAdminIds, notification);

        return result;
    }
}