namespace FitPass.Domain.Events.OwnedPasses;

public class PassExpiredEvent : BaseEvent
{
    public PassExpiredEvent(GymMembershipPass pass)
    {
        Pass = pass;
    }
    public GymMembershipPass Pass { get; }
}
