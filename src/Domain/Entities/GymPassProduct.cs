namespace FitPass.Domain.Entities;

public class GymPassProduct : BaseEntity
{
    public required string GymId { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    //not implemented Stripe properties
    //ProductDefaultPriceDataOptions DefaultPriceData
    //List<string>
    //List<ProductMarketingFeatureOptions> MarketingFeatures
    //string Url
    public required PassType Type { get; set; }
    public required int? TotalUses { get; set; }
    public required int? DaysAfterExpiring { get; set; }
    public required decimal HUFPrice { get; set; }
    public required bool IsActive { get; set; }
    public bool IsCreatedOnStripe { get; set; } = false;
    public string? StripeProductId { get; set; }
    public Gym Gym { get; set; } = null!;

    public DateOnly? GetExpirationDate()
    {
        if (DaysAfterExpiring == null)
        {
            return null;
        }

        var utcNow = DateTimeOffset.UtcNow;

        return new DateOnly(utcNow.Year, utcNow.Month, utcNow.Day).AddDays((int)DaysAfterExpiring);
    }
}
