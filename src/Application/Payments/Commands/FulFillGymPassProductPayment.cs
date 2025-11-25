using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Models;
using FitPass.Application.GymMembershipPasses.DTOs;
using FitPass.Application.GymMemberships.Commands;
using FitPass.Domain.Entities;

namespace FitPass.Application.Payments.Commands;

//Webhook only
public record FulFillGymPassProductPaymentCommand(string UserId, string GymId, string GymPassProductId) : IRequest<Result>;

public class FulFillGymPassProdcutPaymentCommandHandler : IRequestHandler<FulFillGymPassProductPaymentCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly ISender _sender;
    private readonly IClientNotificationSender _notificationSender;

    public FulFillGymPassProdcutPaymentCommandHandler(
        IApplicationDbContext context, 
        ISender sender,
        IClientNotificationSender notificationSender)
    {
        _context = context;
        _sender = sender;
        _notificationSender = notificationSender;
    }
    
    public async Task<Result> Handle(FulFillGymPassProductPaymentCommand command, CancellationToken cancellationToken)
    {
        var product = await _context.GymPassProducts
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == command.GymPassProductId);

        if (product is null)
        {
            return Result.NotFound(nameof(GymPassProduct), ["Failed to fulfill payment because the GymPassProduct was not found."]);
        }

        var gymMembership = await _sender.Send(new GetOrCreateGymMembershipCommand(command.UserId, command.GymId));

        var pass = product.ToGymMembershipPass(gymMembership.Id);

        await _context.GymMembershipPasses.AddAsync(pass);
        await _context.SaveChangesAsync();

        var notification = ClientNotification.Create(
            "Successful payment, pass received!", 
            ClientNotificationType.GymMembershipPassPurchaseSuccessful, 
            pass.MapToDto());
        
        await _notificationSender.SendUserEvent(command.UserId, notification); //TODO create this in Domain event & create PurchaseReceipt

        return Result.Success();
    }
}