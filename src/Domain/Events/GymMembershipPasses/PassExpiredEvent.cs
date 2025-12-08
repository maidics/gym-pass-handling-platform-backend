namespace FitPass.Domain.Events.GymMembershipPasses;

public class PassExpiredEvent : BaseEvent
{
    public PassExpiredEvent(GymMembershipPass pass)
    {
        Pass = pass;
    }
    public GymMembershipPass Pass { get; }
}
