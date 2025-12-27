using FitPass.Domain.Entities;
using FitPass.Domain.Enums;
using FitPass.Domain.ValueObjects;
using NUnit.Framework;
using Shouldly;

namespace FitPass.Domain.UnitTests.Entities;

public class GymPassProductTests
{
    [Test]
    public void ShouldReturnSingleUsePass()
    {
        var product = GymPassProduct.SingleUse("gymId", "name", "description", false, Money.Usd(10));

        product.ShouldSatisfyAllConditions(
            () => product.GymId.ShouldBe("gymId"),
            () => product.Name.ShouldBe("name"),
            () => product.Description.ShouldBe("description"),
            () => product.Type.ShouldBe(PassType.SingleUse),
            () => product.TotalUses.ShouldBe(1),
            () => product.DaysAfterExpires.ShouldBeNull(),
            () => product.IsActive.ShouldBeFalse(),
            () => product.Price.ShouldBe(Money.Usd(10)));
    }

    [Test]
    public void ShouldReturnMultiUsePass()
    {
        var product = GymPassProduct.MultiUse("gymId", "name", "description", 5, true, Money.Usd(10));

        product.ShouldSatisfyAllConditions(
            () => product.GymId.ShouldBe("gymId"),
            () => product.Name.ShouldBe("name"),
            () => product.Description.ShouldBe("description"),
            () => product.Type.ShouldBe(PassType.MultiUse),
            () => product.TotalUses.ShouldBe(5),
            () => product.DaysAfterExpires.ShouldBeNull(),
            () => product.IsActive.ShouldBeTrue(),
            () => product.Price.ShouldBe(Money.Usd(10)));
    }

    [Test]
    public void ShouldReturnUnlimitedUsePass()
    {
        var product = GymPassProduct.UnlimitedUse("gymId", "name", "description", 5, false, Money.Usd(10));

        product.ShouldSatisfyAllConditions(
            () => product.GymId.ShouldBe("gymId"),
            () => product.Name.ShouldBe("name"),
            () => product.Description.ShouldBe("description"),
            () => product.Type.ShouldBe(PassType.Unlimited),
            () => product.TotalUses.ShouldBeNull(),
            () => product.DaysAfterExpires.ShouldBe(5),
            () => product.IsActive.ShouldBeFalse(),
            () => product.Price.ShouldBe(Money.Usd(10)));
    }

    [Test]
    public void ShouldUpdateTotalUses()
    {
        var product = GymPassProduct.MultiUse("gymId", "name", "description", 6, false, Money.Usd(10));

        product.UpdateTotalUsesIfApplicable(10);

        product.TotalUses.ShouldBe(10);
    }

    [Test]
    public void ShouldNotUpdateTotalUses()
    {
        var product = GymPassProduct.SingleUse("gymId", "name", "description", false, Money.Usd(10));

        product.UpdateTotalUsesIfApplicable(10);

        product.TotalUses.ShouldBe(1);
    }

    [Test]
    public void ShouldReturnExpirationDate()
    {
        var utcNow = DateTimeOffset.UtcNow;

        var product = GymPassProduct.UnlimitedUse("gymId", "name", "description", 5, false, Money.Eur(10));

        var expirationDate = product.GetExpirationDate(utcNow);

        expirationDate.ShouldBe(utcNow.AddDays(5));
    }

    [Test]
    public void ShouldReturnGymMembershipPass()
    {
        var product = GymPassProduct.SingleUse("gymId", "name", "description", false, Money.Usd(10));

        var utcNow = DateTimeOffset.UtcNow;

        var pass = product.ToGymMembershipPass("gymMembershipId", "userId", utcNow);

        pass.GymMembershipId.ShouldBe("gymMembershipId");
        pass.UserId.ShouldBe("userId");
        pass.Type.ShouldBe(PassType.SingleUse);
        pass.TotalUses.ShouldBe(1);
        pass.RemainingUses.ShouldBe(1);
        pass.ExpirationDate.ShouldBeNull();
    }
}
