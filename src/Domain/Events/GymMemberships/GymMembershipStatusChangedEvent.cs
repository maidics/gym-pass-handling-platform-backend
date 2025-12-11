namespace FitPass.Domain.Events.GymMemberships;

public class GymMembershipStatusChangedEvent : BaseEvent
{
    public required string UserId { get; init; }
    public required GymMembershipStatus NewStatus { get; init; }
    public required string GymId { get; init; }
}
