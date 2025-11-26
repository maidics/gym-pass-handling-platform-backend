
namespace FitPass.Domain.Events.GymPassProducts;

public class GymPassProductPurchaseFulfillmentFailedEvent : BaseEvent
{
    public GymPassProductPurchaseFulfillmentFailedEvent(string userId, string gymId, string gymPassProductId)
    {
        UserId = userId;
        GymId = gymId;
        GymPassProductId = gymPassProductId;
    }

    public string UserId { get; }
    public string GymId { get; }
    public string GymPassProductId { get; }
}
