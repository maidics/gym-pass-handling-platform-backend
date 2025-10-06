namespace FitPass.Domain.Events.Users;

public class PendingGymAdminRegisteredEvent : BaseEvent
{
    public PendingGymAdminRegisteredEvent(ApplicationUser user)
    {
        User = user;
    }
    public ApplicationUser User { get; set; }
}
