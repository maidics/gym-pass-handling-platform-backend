using FitPass.Domain.Entities;

namespace FitPass.Infrastructure.Stripe.Entities;

public class StripeProduct
{
    public required string Id { get; set; }
    public required string StripePriceId { get; set; }
    public required string GymPassProductId { get; set; }
    public required StripePrice Price { get; set; }
    public GymPassProduct GymPassProduct { get; set; } = null!;
}
