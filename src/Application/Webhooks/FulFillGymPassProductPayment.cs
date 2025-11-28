using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Models;
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
    private readonly TimeProvider _timeProvider;

    public FulFillGymPassProductPaymentCommandHandler(
        IApplicationDbContext context, 
        ISender sender,
        TimeProvider timeProvider)
    {
        _context = context;
        _sender = sender;
        _timeProvider = timeProvider;
    }
    
    public async Task<Result> Handle(FulFillGymPassProductPaymentCommand command, CancellationToken cancellationToken)
    {
        var membership = await _sender.Send(new GetOrCreateGymMembershipCommand(command.UserId, command.GymId));

        var product = await _context.GymPassProducts
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == command.GymPassProductId);

        if (product is null)
        {
            membership.AddDomainEvent(new GymPassProductPurchaseFulfillmentFailedEvent(command.UserId, command.GymId, command.GymPassProductId));

            return Result.NotFound(nameof(GymPassProduct), ["Failed to fulfill payment because the GymPassProduct was not found."]);
        }

        var pass = product.ToGymMembershipPass(membership.Id, command.UserId, _timeProvider.GetUtcNow());

        await _context.GymMembershipPasses.AddAsync(pass);

        pass.AddDomainEvent(new GymPassProductPurchasedEvent(membership, product));

        await _context.SaveChangesAsync();

        return Result.Success();
    }
}