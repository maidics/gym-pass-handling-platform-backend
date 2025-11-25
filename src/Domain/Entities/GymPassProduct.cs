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

    public DateTimeOffset GetExpirationDate(DateTimeOffset utcNow)
    {
        if (DaysAfterExpiring == null)
        {
            throw new InvalidOperationException("Use based pass type does not have an expiration date.");
        }

        return utcNow.AddDays((int)DaysAfterExpiring);
    }

    public GymMembershipPass ToGymMembershipPass(string gymMembershipId, DateTimeOffset utcNow)
    {
        return new GymMembershipPass
        {
            GymMembershipId = gymMembershipId,
            Type = Type,
            TotalUses = TotalUses,
            ExpirationDate = GetExpirationDate(utcNow),
            RemainingUses = TotalUses
        };
    }
}
