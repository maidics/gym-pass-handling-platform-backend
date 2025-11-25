using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Models;
using FitPass.Application.GymMembershipPasses.DTOs;
using FitPass.Application.GymMemberships.Commands;
using FitPass.Domain.Entities;
using FitPass.Domain.Events.GymPassProducts;

namespace FitPass.Application.Payments.Commands;

//Webhook only
public record FulFillGymPassProductPaymentCommand(string UserId, string GymId, string GymPassProductId) : IRequest<Result>;

public class FulFillGymPassProductPaymentCommandHandler : IRequestHandler<FulFillGymPassProductPaymentCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly ISender _sender;
    private readonly IClientNotificationSender _notificationSender;

    public FulFillGymPassProductPaymentCommandHandler(
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

        var membership = await _sender.Send(new GetOrCreateGymMembershipCommand(command.UserId, command.GymId));

        var pass = product.ToGymMembershipPass(membership.Id);

        await _context.GymMembershipPasses.AddAsync(pass);

        pass.AddDomainEvent(new GymPassProductPurchasedEvent(membership, product));
        
        await _context.SaveChangesAsync();

        return Result.Success();
    }
}