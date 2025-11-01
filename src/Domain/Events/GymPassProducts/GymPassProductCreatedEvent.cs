namespace FitPass.Domain.Events.GymPassProducts;

public class GymPassProductCreatedEvent : BaseEvent
{
    public GymPassProductCreatedEvent(GymPassProduct gymPassProduct)
    {
        GymPassProduct = gymPassProduct;
    }
    public GymPassProduct GymPassProduct { get; }
}
