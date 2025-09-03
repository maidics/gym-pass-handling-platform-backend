namespace FitPass.Domain;

public class PassUsedEvent : BaseEvent
{
    public PassUsedEvent(Pass pass)
    {
        Pass = pass;
        Timestamp = DateTimeOffset.UtcNow;
    }
    public Pass Pass { get; }
    public DateTimeOffset Timestamp { get; }
}