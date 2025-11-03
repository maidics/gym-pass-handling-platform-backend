namespace FitPass.Domain.Entities;

public class PaymentProduct
{
    public required string Id { get; set; }
    public required string StripePriceId { get; set; }
    public required string GymPassProductId { get; set; }
    public required PaymentPrice Price { get; set; }
    public GymPassProduct GymPassProduct { get; set; } = null!;
}
