namespace FitPass.Domain.Events.GymPassProducts;

public record WebhookGymPassProductPurchaseFulfillmentFailedEvent(
    string UserId,
    string GymId,
    string GymPassProductId,
    string ReceiptId) : BaseEvent;
