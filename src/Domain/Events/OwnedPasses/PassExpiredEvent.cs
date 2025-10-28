namespace FitPass.Domain.Events;

public class PassExpiredEvent : BaseEvent
{
    public PassExpiredEvent(GymMembershipPass pass)
    {
        Pass = pass;
    }
    public GymMembershipPass Pass { get; }
}
