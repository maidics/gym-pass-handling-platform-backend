using FitPass.Application.Common.Interfaces;
using FitPass.Domain.Strings;

namespace Fitpass.Application.ApplicationUsers.EventHandlers;

/*
public class UserRegisteredEventHandler : INotificationHandler<UserRegisteredEvent>
{
    private readonly ILocalDevEmailService _localDevEmailService;

    public UserRegisteredEventHandler(ILocalDevEmailService localDevEmailService)
    {
        _localDevEmailService = localDevEmailService;
    }
    public async Task Handle(UserRegisteredEvent notification, CancellationToken cancellationToken)
    {
        await _localDevEmailService.SendEmailAsync(notification.User.Email!, EmailSubjects.Welcome(), EmailBodies.Welcome(notification.User.FirstName));
    }
}
*/