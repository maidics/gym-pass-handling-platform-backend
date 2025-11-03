using FitPass.Infrastructure.Identity;

namespace FitPass.Infrastructure.Stripe.Entities;

public class StripeCustomer
{
    public required string Id { get; set; }
    public required string ApplicationUserId { get; set; }
    public ApplicationUser ApplicationUser { get; set; } = null!;
}
