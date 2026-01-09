using FitPass.Domain.Events.Users;
using FitPass.Application.Users.Commands.Emails;

namespace FitPass.Application.Users.EventHandlers;

public class UserRegisteredEventHandler : INotificationHandler<UserRegisteredEvent>
{
    private readonly ISender _sender;

    public UserRegisteredEventHandler(
        ISender sender)
    {
        _sender = sender;
    }
    
    public async Task Handle(UserRegisteredEvent notification, CancellationToken cancellationToken)
    {
        var command = new SendAccountActivationEmailCommand(notification.UserEmail, notification.UserId);
        
        await _sender.Send(command);
    }
}
