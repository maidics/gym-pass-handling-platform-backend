namespace FitPass.Infrastructure.Stripe;

public class StripeSettings
{
    public required string Key { get; init; }
    public required StripeTaxCodes TaxCodes { get; init; }
    public required AccountLinkPaths AccountLinkPaths { get; init; }
    public required string ClientName { get; init; }

    public string GetAccountLinkReturnPath(string clientAppBaseUrl, string gymId)
    {
        return $"{clientAppBaseUrl}{AccountLinkPaths.Return}/{gymId}";
    }
    
    public string GetAccountLinkRefreshPath(string clientAppBaseUrl, string gymId)
    {
        return $"{clientAppBaseUrl}{AccountLinkPaths.Refresh}/{gymId}";
    }
}

public class StripeTaxCodes
{
    public required string Membership {  get; init; }
    public required string SingleUseAccess { get; init; }
}

public class AccountLinkPaths
{
    public required string Return { get; init; }
    public required string Refresh { get; init; }
}
