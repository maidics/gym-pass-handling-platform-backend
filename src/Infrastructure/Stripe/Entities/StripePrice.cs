namespace FitPass.Infrastructure.Stripe.Entities;

public class StripePrice
{
    public required string Id { get; set; }
    public required decimal Amount { get; set; }
    public required string Currency { get; set; }
    public ICollection<StripeProduct> Products { get; set; } = [];
}
