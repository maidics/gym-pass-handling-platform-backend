namespace FitPass.Domain.Events.Users;

public record UserRegisteredEvent(
    string UserId,
    string UserEmail,
    string UserFirstName,
    bool ByGymEmployee) : BaseEvent;
