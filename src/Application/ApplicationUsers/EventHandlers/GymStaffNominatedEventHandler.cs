using FitPass.Application.Common.Interfaces;
using FitPass.Domain.Events.Users;
using FitPass.Domain.Strings;

namespace FitPass.Application.ApplicationUsers.EventHandlers;

public class GymStaffNominatedEventHandler : INotificationHandler<GymStaffNominatedEvent>
{
    private readonly ILocalDevEmailService _localDevEmailService;
    public GymStaffNominatedEventHandler(ILocalDevEmailService localDevEmailService)
    {
        _localDevEmailService = localDevEmailService;
    }
    public async Task Handle(GymStaffNominatedEvent notification, CancellationToken cancellationToken)
    {
        await _localDevEmailService.SendEmailAsync(notification.User.Email!, EmailSubjects.Placeholder(), EmailBodies.Placeholder());
    }
}