namespace FitPass.Domain;

public class PassUsedEvent : BaseEvent
{
    public PassUsedEvent(OwnedPass pass)
    {
        Pass = pass;
        Timestamp = DateTimeOffset.UtcNow;
    }
    public OwnedPass Pass { get; }
    public DateTimeOffset Timestamp { get; }
}