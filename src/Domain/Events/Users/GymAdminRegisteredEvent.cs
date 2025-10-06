namespace FitPass.Domain.Events.Users;

public class GymAdminRegisteredEvent : BaseEvent
{
    public GymAdminRegisteredEvent(ApplicationUser user)
    {
        User = user;
    }
    public ApplicationUser User { get; set; }
}
