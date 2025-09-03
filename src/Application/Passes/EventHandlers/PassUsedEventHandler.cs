using FitPass.Domain;
using Microsoft.Extensions.Logging;

namespace FitPass.Application;

public class PassUsedEventHandler : INotificationHandler<PassUsedEvent>
{
    private readonly ILogger<PassUsedEventHandler> _logger;

    public PassUsedEventHandler(ILogger<PassUsedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(PassUsedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("FitPass Domain Event: {DomainEvent}", notification.GetType().Name);

        return Task.CompletedTask;
    }
}