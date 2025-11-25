namespace FitPass.Domain.Events.GymPassProducts;

public class GymPassProductPurchasedEvent : BaseEvent
{
    public GymPassProductPurchasedEvent(GymMembership gymMembership, GymPassProduct gymPassProduct)
    {
        GymMembership = gymMembership;
        GymPassProduct = gymPassProduct;
    }

    public GymMembership GymMembership { get; init; }
    public GymPassProduct GymPassProduct { get; init; }
}
