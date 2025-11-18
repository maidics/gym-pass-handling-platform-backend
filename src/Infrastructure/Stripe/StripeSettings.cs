namespace FitPass.Infrastructure.Stripe;

public class StripeSettings
{
    public string TestKey { get; init; } = string.Empty;
    public string Currency { get; init; } = string.Empty;
    public TaxCodeSettings TaxCodeSettings { get; init; } = null!;
}

public class TaxCodeSettings
{
    public required string Membership {  get; init; }
    public required string SingleUseAccess { get; init; }
}
