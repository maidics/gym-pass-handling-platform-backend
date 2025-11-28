using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Models;
using FitPass.Application.Users.Queries;
using FitPass.Domain.Entities;

namespace FitPass.Application.Webhooks.Products;

//webhook only
public record SyncPaymentProviderProductDeletedCommand(
    string PaymentProductId,
    string PaymentAccountId) : IRequest<Result>;

public class SyncPaymentProviderProductDeletedCommandHandler : IRequestHandler<SyncPaymentProviderProductDeletedCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IClientNotificationSender _notificationSender;
    private readonly ISender _sender;

    public SyncPaymentProviderProductDeletedCommandHandler(
        IApplicationDbContext context,
        IClientNotificationSender notificationSender,
        ISender sender)
    {
        _context = context;
        _notificationSender = notificationSender;
        _sender = sender;
    }

    //TODO: make sure integrity stays in tact here
    public async Task<Result> Handle(SyncPaymentProviderProductDeletedCommand command, CancellationToken cancellationToken)
    {
        var product = await _context.GymPassProducts
            .FirstOrDefaultAsync(x => x.PaymentIdentity.Id == command.PaymentProductId);

        if (product is not null)
        {
            _context.GymPassProducts.Remove(product);

            await _context.SaveChangesAsync();
        }

        var gymAdminIds = await _sender.Send(new GetAllGymAdminIdsByTenantPaymentAccountIdQuery(command.PaymentAccountId));

        string message;

        if (product is null)
        {
            message = "No GymPassProduct found in our system, but we detected a product delete on Stripe.";
        } else
        {
            message = $"Deleted GymPassProduct that was deleted from Stripe: {product.Name}";
        }

        var notifcation = ClientNotification.Create(message, ClientNotificationType.GymPassProductSynced);

        await _notificationSender.SendAsync(gymAdminIds, notifcation);

        return product is null ? Result.NotFound(nameof(GymPassProduct)) : Result.Success();
    }
}