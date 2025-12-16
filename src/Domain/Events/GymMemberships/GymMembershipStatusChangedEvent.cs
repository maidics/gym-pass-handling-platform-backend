namespace FitPass.Domain.Events.GymMemberships;

public record GymMembershipStatusChangedEvent(
    string UserId,
    GymMembershipStatus NewStatus,
    string GymId) : BaseEvent;
