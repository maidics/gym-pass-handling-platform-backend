namespace FitPass.Infrastructure.Stripe;

public class StripeSettings
{
    public string Key { get; init; } = string.Empty;
    public required StripeTaxCodeSettings TaxCodeSettings { get; init; }
    public required StripeAccountLinkSettings AccountLinks { get; init; }
}

public class StripeTaxCodeSettings
{
    public required string Membership {  get; init; }
    public required string SingleUseAccess { get; init; }
}

public class StripeAccountLinkSettings
{
    public required string ReturnUrl { get; init; }
    public required string RefreshUrl { get; init; }
}
