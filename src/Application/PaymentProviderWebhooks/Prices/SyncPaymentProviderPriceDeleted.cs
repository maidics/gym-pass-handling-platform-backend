using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Interfaces.Payment;
using FitPass.Application.Common.Models;
using FitPass.Application.Users.Queries;

namespace FitPass.Application.Webhooks.Prices;

//webhook only
public record SyncPaymentProviderPriceDeletedCommand(
    string PaymentPriceId,
    string PaymentAccountId
) : IRequest<Result>;

public class SyncPaymentProviderPriceDeletedCommandHandler : IRequestHandler<SyncPaymentProviderPriceDeletedCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IClientNotificationSender _notificationSender;
    private readonly IPaymentProductService _productService;
    private readonly IPaymentPriceService _priceService;
    private readonly ISender _sender;

    public SyncPaymentProviderPriceDeletedCommandHandler(
        IApplicationDbContext context,
        IClientNotificationSender notificationSender,
        IPaymentProductService productService,
        IPaymentPriceService priceService,
        ISender sender)
    {
        _context = context;
        _productService = productService;
        _priceService = priceService;
        _notificationSender = notificationSender;
        _sender = sender;
    }

    public async Task<Result> Handle(SyncPaymentProviderPriceDeletedCommand command, CancellationToken cancellationToken)
    {
        var product = await _context.GymPassProducts
            .Include(x => x.PaymentIdentity)
            .FirstOrDefaultAsync(x => x.PaymentIdentity.PriceId == command.PaymentPriceId);

        if (product is not null)
        {
            var result = await _priceService.CreatePriceAsync(
                product.PaymentIdentity.Id, 
                product.Price, 
                product.IsActive, 
                command.PaymentAccountId);

            if (!result.Succeeded)
            {
                await _priceService.UpdateActiveStatusAsync(command.PaymentPriceId, false);
                await _productService.DeleteProductAsync(product.PaymentIdentity.Id);

                _context.GymPassProducts.Remove(product);
                await _context.SaveChangesAsync();
            }
        }

        var gymAdminIds = await _sender.Send(new GetAllGymAdminIdsByTenantPaymentAccountIdQuery(command.PaymentAccountId));

        var message = "Please infer from deleting prices on Stripe. If there's a connected GymPassProduct the price will be reset and if this reset is not successful then the GymPassProduct will be deleted.";

        var notification = ClientNotification.Create(message, ClientNotificationType.PaymentProviderPriceSynced);

        await _notificationSender.SendAsync(gymAdminIds, notification);

        return Result.Success();
    }
}