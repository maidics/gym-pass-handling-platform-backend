using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Models;
using FitPass.Application.GymPassProducts.DTOs;
using FitPass.Domain.Entities.Payment;
using FitPass.Domain.Events.GymPassProducts;

namespace FitPass.Application.Payments.EventHandlers;

public class GymPassProductPurchasedEventHandler : INotificationHandler<GymPassProductPurchasedEvent>
{
    private readonly TimeProvider _timeProvider;
    private readonly IApplicationDbContext _context;
    private readonly IClientNotificationSender _clientNotificationSender;
    private readonly IEmailService _emailService;

    public GymPassProductPurchasedEventHandler(
        TimeProvider timeProvider,
        IApplicationDbContext context,
        IClientNotificationSender clientNotificationSender,
        IEmailService emailService
    )
    {
        _timeProvider = timeProvider;
        _context = context;
        _clientNotificationSender = clientNotificationSender;
        _emailService = emailService;
    }

    public async Task Handle(GymPassProductPurchasedEvent notification, CancellationToken cancellationToken)
    {
        var purchaseReceipt = new PurchaseReceipt
        {
            UserId = notification.GymMembership.UserId,
            GymMembershipId = notification.GymMembership.Id,
            GymId = notification.GymMembership.GymId,
            CreatedOn = _timeProvider.GetUtcNow(),
            GymPassProductId = notification.GymPassProduct.Id,
            Spent = notification.GymPassProduct.Price
        };

        await _context.PurchaseReceipts.AddAsync(purchaseReceipt);
        await _context.SaveChangesAsync();

        await _emailService.SendEmailAsync("TODO", "TODO", "TODO"); //TODO send email here with receipt

        var clientNotification = ClientNotification.Create(
            "Successful payment, pass received!", 
            ClientNotificationType.GymMembershipPassPurchaseSuccessful, 
            notification.GymPassProduct.MapToDto());
        
        await _clientNotificationSender.Send(purchaseReceipt.UserId, clientNotification);
    }
}
