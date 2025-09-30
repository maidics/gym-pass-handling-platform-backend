namespace FitPass.Domain.Events;

public class PassExpiredEvent : BaseEvent
{
    public PassExpiredEvent(OwnedPass pass)
    {
        Pass = pass;
    }
    public OwnedPass Pass { get; }
}
