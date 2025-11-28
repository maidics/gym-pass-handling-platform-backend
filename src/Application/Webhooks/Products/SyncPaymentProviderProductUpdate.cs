using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Models;
using FitPass.Application.Users.Queries;
using FitPass.Domain.Entities;

namespace FitPass.Application.Webhooks.Products;

//webhook only
public record SyncPaymentProviderProductUpdateCommand(
    string PaymentProductId,
    string Name,
    string Description,
    bool IsActive,
    string PaymentAccountId
) : IRequest<Result>;

public class SyncPaymentProviderProductUpdateCommandHandler : IRequestHandler<SyncPaymentProviderProductUpdateCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IClientNotificationSender _notificationSender;
    private readonly ISender _sender;

    public SyncPaymentProviderProductUpdateCommandHandler(
        IApplicationDbContext context,
        IClientNotificationSender notificationSender,
        ISender sender)
    {
        _context = context;
        _notificationSender = notificationSender;
        _sender = sender;
    }

    public async Task<Result> Handle(SyncPaymentProviderProductUpdateCommand command, CancellationToken cancellationToken)
    {
        var product = await _context.GymPassProducts
            .FirstOrDefaultAsync(x => x.PaymentIdentity.Id == command.PaymentProductId);

        Guard.Against.Null(product, nameof(GymPassProduct), "Webhook specified product does not exist.");

        bool isTheSame = product.Name == command.Name && 
            product.Description == command.Description &&
            product.IsActive == command.IsActive;

        if (isTheSame)
        {
            return Result.Success();
        }

        product.Name = command.Name;
        product.Description = command.Description;
        product.IsActive = command.IsActive;
        
        await _context.SaveChangesAsync();

        var gymAdminIds = await _sender.Send(new GetAllGymAdminIdsByTenantPaymentAccountIdQuery(command.PaymentAccountId));

        var notification = ClientNotification.Create("GymPassProduct updated.", ClientNotificationType.GymPassProductSynced);

        await _notificationSender.SendAsync(gymAdminIds, notification);

        return Result.Success();
    }
}