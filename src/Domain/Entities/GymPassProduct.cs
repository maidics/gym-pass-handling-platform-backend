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
    public int? DaysAfterExpiring { get; private set; }
    public bool IsActive { get; set; }
    public Money Price { get; set; }
    public ProductPaymentIdentity PaymentIdentity { get; set; }
    public Gym Gym { get; set; } = null!;

    private GymPassProduct() //for EF core
    {
        GymId = string.Empty;
        Name = string.Empty;
        Description = string.Empty;
        Price = Money.Zero("usd");
        PaymentIdentity = new ProductPaymentIdentity
        {
            GymPassProductId = string.Empty,
            PriceId = string.Empty
        };
    }

    private GymPassProduct(
        string gymId, 
        string name, 
        string description, 
        PassType type, 
        int? totalUses, 
        int? daysAfteExpiring, 
        bool isActive, 
        Money price, 
        ProductPaymentIdentity paymentIdentity)
    {
        GymId = gymId;
        Name = name;
        Description = description;
        Type = type;
        TotalUses = totalUses;
        DaysAfterExpiring = daysAfteExpiring;
        IsActive = isActive;
        Price = price;
        PaymentIdentity = paymentIdentity; 
    }

    public static GymPassProduct SingleUse(string gymId, string name, string description, bool isActive, Money price, ProductPaymentIdentity paymentIdentity)
    {
        return new GymPassProduct(gymId, name, description, PassType.SingleUse, 1, null, isActive, price, paymentIdentity);
    }

    public static GymPassProduct MultiUse(string gymId, string name, string description, int totalUses, bool isActive, Money price, ProductPaymentIdentity paymentIdentity)
    {
        if (totalUses < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(totalUses));
        }

        return new GymPassProduct(gymId, name, description, PassType.MultiUse, totalUses, null, isActive, price, paymentIdentity);
    }

    public static GymPassProduct UnlimitedUse(string gymId, string name, string description, int daysAfterExpiring, bool isActive, Money price, ProductPaymentIdentity paymentIdentity)
    {
        if (daysAfterExpiring < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(daysAfterExpiring));
        }

        return new GymPassProduct(gymId, name, description, PassType.Unlimited, null, daysAfterExpiring, isActive, price, paymentIdentity);
    }

    public GymPassProduct UpdateTotalUses(int totalUses)
    {
        if (Type != PassType.MultiUse)
        {
            throw new InvalidOperationException("Cannot update total uses to non multi use passes.");
        }

        TotalUses = totalUses;

        return this;
    }

    public GymPassProduct UpdateDaysAfterExpiring(int daysAfterExpiring)
    {
        if (Type != PassType.Unlimited)
        {
            throw new InvalidOperationException("Cannot update days after expiring to non unlimited use passes.");
        }

        DaysAfterExpiring = daysAfterExpiring;

        return this;
    }

    public DateTimeOffset GetExpirationDate(DateTimeOffset utcNow)
    {
        if (DaysAfterExpiring == null)
        {
            throw new InvalidOperationException("Use based pass type does not have an expiration date.");
        }

        return utcNow.AddDays((int)DaysAfterExpiring);
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
