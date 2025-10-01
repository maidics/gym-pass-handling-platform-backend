using FitPass.Application.Common.Interfaces;
using FitPass.Domain.Events;

namespace FitPass.Application.OwnedPasses.EventHandlers;

public class PassExpiredEventHandler : INotificationHandler<PassExpiredEvent>
{
    private readonly IApplicationDbContext _context;
    public PassExpiredEventHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(PassExpiredEvent notification, CancellationToken cancellationToken)
    {
        var pass = await _context
            .OwnedPasses
            .FirstOrDefaultAsync(op => op.Id == notification.Pass.Id);

        if (pass == null)
        {
            return;
        }

        _context.OwnedPasses.Remove(pass);
        await _context.SaveChangesAsync();
    }
}
