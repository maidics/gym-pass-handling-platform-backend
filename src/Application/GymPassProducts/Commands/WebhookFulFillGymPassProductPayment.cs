using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Models;
using FitPass.Application.GymMemberships.Commands;
using FitPass.Domain.Entities;
using FitPass.Domain.Entities.Payment;
using FitPass.Domain.Events.GymPassProducts;

namespace FitPass.Application.GymPassProducts.Commands;

//Webhook only
public record WebhookFulFillGymPassProductPaymentCommand(
    string UserId,
    string GymId,
    string GymPassProductId
) : IRequest<Result>;

public class WebhookFulFillGymPassProductPaymentCommandHandler
    : IRequestHandler<WebhookFulFillGymPassProductPaymentCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly ISender _sender;
    private readonly TimeProvider _timeProvider;

    public WebhookFulFillGymPassProductPaymentCommandHandler(
        IApplicationDbContext context,
        ISender sender,
        TimeProvider timeProvider
    )
    {
        _context = context;
        _sender = sender;
        _timeProvider = timeProvider;
    }

    public async Task<Result> Handle(
        WebhookFulFillGymPassProductPaymentCommand command,
        CancellationToken cancellationToken
    )
    {
        var membership = await _sender.Send(
            new GetOrCreateGymMembershipCommand(command.UserId, command.GymId),
            cancellationToken
        );

        var product = await _context
            .GymPassProducts.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == command.GymPassProductId, cancellationToken);

        if (product is null)
        {
            var failedReceipt = new PurchaseReceipt
            {
                UserId = command.UserId,
                GymId = command.GymId,
                GymPassProductId = command.GymPassProductId,
                PurchaseSucceeded = false,
                CreatedOn = _timeProvider.GetUtcNow(),
                Spent = null, //Note: because we cannot store Money spent here a user has to send a bank receipt to show proof of purchase if this fails
            };

            await _context.PurchaseReceipts.AddAsync(failedReceipt, cancellationToken);

            membership.AddDomainEvent(
                new WebhookGymPassProductPurchaseFulfillmentFailedEvent(
                    command.UserId,
                    command.GymId,
                    command.GymPassProductId,
                    failedReceipt.Id
                )
            );

            await _context.SaveChangesAsync(cancellationToken);

            return Result.NotFound($"{nameof(GymPassProduct)} not found.");
        }

        var pass = product.ToGymMembershipPass(
            membership.Id,
            command.UserId,
            _timeProvider.GetUtcNow()
        );

        var receipt = new PurchaseReceipt
        {
            UserId = command.UserId,
            GymId = command.GymId,
            GymPassProductId = command.GymPassProductId,
            PurchaseSucceeded = true,
            CreatedOn = _timeProvider.GetUtcNow(),
            Spent = product.Price,
        };

        await _context.GymMembershipPasses.AddAsync(pass, cancellationToken);
        await _context.PurchaseReceipts.AddAsync(receipt, cancellationToken);

        pass.AddDomainEvent(new WebhookGymPassProductPurchasedEvent(command.UserId, command.GymId));

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
