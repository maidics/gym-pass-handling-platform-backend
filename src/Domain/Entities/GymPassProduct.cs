using FitPass.Domain.Entities.Payment;
using FitPass.Domain.ValueObjects;

namespace FitPass.Domain.Entities;

//TODO: add factory method for this
public class GymPassProduct : BaseAuditableEntity
{
    //not implemented Stripe properties
    //ProductDefaultPriceDataOptions DefaultPriceData
    //List<string>
    //List<ProductMarketingFeatureOptions> MarketingFeatures
    //string Url
    public string GymId { get; private set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public PassType Type { get; private set; }
    public int? TotalUses { get; private set; }
    public int? DaysAfterExpires { get; private set; }
    public bool IsActive { get; set; }
    public Money Price { get; set; }
    public ProductPaymentIdentity PaymentIdentity { get; set; } = null!;
    public Gym Gym { get; set; } = null!;

    private GymPassProduct() //for EF core
    {
        GymId = string.Empty;
        Name = string.Empty;
        Description = string.Empty;
        Price = Money.Usd(10);
    }

    private GymPassProduct(
        string gymId, 
        string name, 
        string description, 
        PassType type, 
        int? totalUses, 
        int? daysAfteExpires, 
        bool isActive, 
        Money price)
    {
        GymId = gymId;
        Name = name;
        Description = description;
        Type = type;
        TotalUses = totalUses;
        DaysAfterExpires = daysAfteExpires;
        IsActive = isActive;
        Price = price;
    }

    public static GymPassProduct SingleUse(string gymId, string name, string description, bool isActive, Money price)
    {
        return new GymPassProduct(
            gymId, 
            name, 
            description, 
            PassType.SingleUse, 
            1, 
            null, 
            isActive, 
            price);
    }

    public static GymPassProduct MultiUse(string gymId, string name, string description, int totalUses, bool isActive, Money price)
    {
        if (totalUses < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(totalUses));
        }

        return new GymPassProduct(gymId, name, description, PassType.MultiUse, totalUses, null, isActive, price);
    }

    public static GymPassProduct UnlimitedUse(string gymId, string name, string description, int daysAfterExpiring, bool isActive, Money price)
    {
        if (daysAfterExpiring < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(daysAfterExpiring));
        }

        return new GymPassProduct(gymId, name, description, PassType.Unlimited, null, daysAfterExpiring, isActive, price);
    }

    public GymPassProduct UpdateTotalUsesIfApplicable(int? totalUses)
    {
        if (Type == PassType.MultiUse)
        {
            TotalUses = totalUses;
        }

        return this;
    }

    public GymPassProduct UpdateDaysAfterExpiringIfApplicable(int? daysAfterExpiring)
    {
        if (Type == PassType.Unlimited)
        {
            DaysAfterExpires = daysAfterExpiring;
        }

        return this;
    }

    public DateTimeOffset? GetExpirationDate(DateTimeOffset utcNow)
    {
        if (DaysAfterExpires == null)
        {
            return null;
        }

        return utcNow.AddDays((int)DaysAfterExpires);
    }

    public GymMembershipPass ToGymMembershipPass(string gymMembershipId, string userId, DateTimeOffset utcNow)
    {
        return new GymMembershipPass
        {
            GymMembershipId = gymMembershipId,
            UserId = userId,
            Type = Type,
            TotalUses = TotalUses,
            ExpirationDate = GetExpirationDate(utcNow),
            RemainingUses = TotalUses
        };
    }
}
