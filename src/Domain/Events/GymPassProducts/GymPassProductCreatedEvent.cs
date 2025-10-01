namespace FitPass.Domain.Events;

public class GymPassProductCreatedEvent : BaseEvent
{
    public GymPassProductCreatedEvent(GymPassProduct gymPassProduct)
    {
        GymPassProduct = gymPassProduct;
    }
    public GymPassProduct GymPassProduct { get; }
}
