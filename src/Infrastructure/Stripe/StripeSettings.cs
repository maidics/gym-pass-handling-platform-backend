namespace FitPass.Infrastructure.Stripe;

public class StripeSettings
{
    public required string TestKey { get; init; }
    public required string Currency {  get; init; }
    public required TaxCodeSettings TaxCodeSettings { get; init; }
}

public class TaxCodeSettings
{
    public required string Membership {  get; init; }
    public required string SingleUseAccess { get; init; }
}
