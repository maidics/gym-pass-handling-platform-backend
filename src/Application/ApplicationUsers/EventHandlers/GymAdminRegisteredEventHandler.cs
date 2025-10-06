using FitPass.Application.Common.Interfaces;
using FitPass.Domain.Events.Users;
using FitPass.Domain.Strings;

namespace FitPass.Application.ApplicationUsers.EventHandlers;

public class GymAdminRegisteredEventHandler : INotificationHandler<GymAdminRegisteredEvent>
{
    private readonly ILocalDevEmailService _localDevEmailService;
    public GymAdminRegisteredEventHandler(ILocalDevEmailService localDevEmailService)
    {
        _localDevEmailService = localDevEmailService;
    }
    public async Task Handle(GymAdminRegisteredEvent notification, CancellationToken cancellationToken)
    {
        await _localDevEmailService.SendEmailAsync(notification.User.Email!, EmailBodies.Placeholder(), EmailBodies.Placeholder());
    }
}
