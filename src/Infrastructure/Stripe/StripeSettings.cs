namespace FitPass.Infrastructure.Stripe;

public class StripeSettings
{
    public string Key { get; init; } = string.Empty;
    public required StripeTaxCodes TaxCodes { get; init; }
    public required AccountLinks AccountLinks { get; init; }
}

public class StripeTaxCodes
{
    public required string Membership {  get; init; }
    public required string SingleUseAccess { get; init; }
}

public class AccountLinks
{
    public required string ReturnUrl { get; init; }
    public required string RefreshUrl { get; init; }
}
