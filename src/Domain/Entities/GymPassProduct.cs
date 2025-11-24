using FitPass.Domain.Entities.Payment;
using FitPass.Domain.ValueObjects;

namespace FitPass.Domain.Entities;

public class GymPassProduct : BaseAuditableEntity
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
    public required bool IsActive { get; set; }
    public required Money Price { get; set; }
    public required ProductPaymentIdentity PaymentIdentity { get; set; }
    public Gym Gym { get; set; } = null!;

    public DateOnly GetExpirationDate()
    {
        if (DaysAfterExpiring == null)
        {
            throw new InvalidOperationException("Use based pass type does not have an expiration date.");
        }

        var utcNow = DateTimeOffset.UtcNow;

        return new DateOnly(utcNow.Year, utcNow.Month, utcNow.Day).AddDays((int)DaysAfterExpiring);
    }
}
