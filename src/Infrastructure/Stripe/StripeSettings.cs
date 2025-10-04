namespace FitPass.Infrastructure.Stripe;

public class StripeSettings
{
    public const string SectionName = "Stripe";

    public string TestKey { get; init; } = string.Empty;
    public string Currency {  get; init; } = string.Empty;
    public TaxCodeSettings TaxCodeSettings { get; init; } = new(); 
}

public class TaxCodeSettings
{
    public string Membership {  get; init; } = string.Empty;
    public string SingleUseAccess {  get; init; } = string.Empty;
}
