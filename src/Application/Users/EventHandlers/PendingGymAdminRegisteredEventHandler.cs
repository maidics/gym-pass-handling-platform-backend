using FitPass.Application.Common.Interfaces;
using FitPass.Domain.Strings;

namespace FitPass.Application.ApplicationUsers.EventHandlers;

/*
public class PendingGymAdminRegisteredEventHandler : INotificationHandler<PendingGymAdminRegisteredEvent>
{
    private readonly ILocalDevEmailService _localDevEmailService;
    public PendingGymAdminRegisteredEventHandler(ILocalDevEmailService localDevEmailService)
    {
        _localDevEmailService = localDevEmailService;
    }
    public async Task Handle(PendingGymAdminRegisteredEvent notification, CancellationToken cancellationToken)
    {
        await _localDevEmailService.SendEmailAsync(notification.User.Email!, EmailBodies.Placeholder(), EmailBodies.Placeholder());
    }
}
*/