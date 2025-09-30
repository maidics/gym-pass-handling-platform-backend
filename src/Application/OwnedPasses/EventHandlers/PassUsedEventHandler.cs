using FitPass.Application.Common.Interfaces;

namespace FitPass.Application.OwnedPasses.EventHandlers;

public class PassExpiredEvent : INotificationHandler<Domain.Events.PassExpiredEvent>
{
    private readonly IApplicationDbContext _context;
    public PassExpiredEvent(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(Domain.Events.PassExpiredEvent notification, CancellationToken cancellationToken)
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
