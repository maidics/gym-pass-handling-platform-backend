namespace FitPass.Domain.Events.Gyms;

public record GymStatusUpdatedByAppAdminEvent(
    string GymId,
    GymStatus NewStatus,
    string Rationale,
    string GymName) : BaseEvent;
