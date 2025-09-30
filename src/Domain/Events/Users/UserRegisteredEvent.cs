namespace FitPass.Domain.Events.Users;

public class UserRegisteredEvent : BaseEvent
{
    public UserRegisteredEvent(ApplicationUser user)
    {
        User = user;
    }
    
    public ApplicationUser User { get; }
}