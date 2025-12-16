namespace FitPass.Domain.Events.GymPassProducts;

public record WebhookGymPassProductPurchasedEvent(
    string UserId,
    string GymId) : BaseEvent;
