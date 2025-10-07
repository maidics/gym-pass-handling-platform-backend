namespace FitPass.Domain.Events.Users;

public class GymStaffNominatedEvent : BaseEvent
{
    public GymStaffNominatedEvent(ApplicationUser user)
    {
        User = user;
    }

    public ApplicationUser User { get; }
}