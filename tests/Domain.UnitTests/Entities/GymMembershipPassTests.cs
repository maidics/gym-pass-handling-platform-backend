using FitPass.Domain.Entities;
using FitPass.Domain.Enums;
using FitPass.Domain.Events.OwnedPasses;
using NUnit.Framework;
using Shouldly;

namespace FitPass.Domain.UnitTests.Entities;

public class GymMembershipPassTests
{
    [TestCase(PassType.SingleUse, 1, 1, null, true)]
    [TestCase(PassType.MultiUse, 3, 1, null, true)]
    [TestCase(PassType.Unlimited, null, null, -1, false)]
    [TestCase(PassType.SingleUse, 1, 0, null, false)]
    [TestCase(PassType.MultiUse, 10, 0, null, false)]
    public void IsUsableShouldReturnCorrectValue(PassType passType, int? totalUses, int? remainingUses, double? expirationDaysFromNow, bool expected)
    {
        var now = DateTimeOffset.UtcNow;

        var pass = new GymMembershipPass
        {
            GymMembershipId = "id",
            UserId = "userId",
            Type = passType,
            TotalUses = totalUses,
            RemainingUses = remainingUses,
            ExpirationDate = GetExpirationDate(expirationDaysFromNow)
        };

        pass.IsValid(now).ShouldBe(expected);
    }

    [TestCase(PassType.SingleUse, 1, 1, null, PassUseResult.Success)]
    [TestCase(PassType.MultiUse, 2, 1, null, PassUseResult.Success)]
    [TestCase(PassType.Unlimited, null, null, 1, PassUseResult.Success)]
    [TestCase(PassType.SingleUse, 1, 0, null, PassUseResult.Expired)]
    [TestCase(PassType.MultiUse, 2, 0, null, PassUseResult.Expired)]
    [TestCase(PassType.Unlimited, null, null, -3, PassUseResult.Expired)]
    public void ShouldReturnCorrectGymPassUsageForSingleUsePass(
        PassType passType,
        int? totalUses,
        int? remainingUses,
        double? expirationDaysFromNow,
        PassUseResult passUseResult)
    {
        var now = DateTimeOffset.UtcNow;
        DateTimeOffset? expirationDate = GetExpirationDate(expirationDaysFromNow);

        var pass = new GymMembershipPass
        {
            Id = "id",
            GymMembershipId = "gymMembershipId",
            UserId = "userId",
            Type = passType,
            TotalUses = totalUses,
            RemainingUses = remainingUses,
            ExpirationDate = expirationDate,
            GymMembership = new GymMembership
            {
                UserId = "userId",
                GymId = "gymId"
            }
        };

        var usage = pass.Use("locker number", now);

        usage.UserId.ShouldBe("userId");
        usage.GymId.ShouldBe("gymId");
        usage.PassId.ShouldBe("id");
        usage.PassType.ShouldBe(passType);
        usage.TotalPassUses.ShouldBe(totalUses);
        usage.RemainingPassUses.ShouldBe(remainingUses == null ? null : remainingUses - 1);
        usage.PassExpirationDate.ShouldBe(expirationDate);
        usage.PassUseResult.ShouldBe(passUseResult);
        usage.LockerNumber.ShouldBe("locker number");
    }

    [Test]
    public void ShouldThrowIfGymMembershipNavigationPropertyIsNotLoaded()
    {
        var pass = new GymMembershipPass
        {
            Id = "id",
            GymMembershipId = "gymMembershipId",
            UserId = "userId",
            Type = PassType.SingleUse,
            TotalUses = 1,
            RemainingUses = 1,
            ExpirationDate = null
        };

        Should.Throw<ArgumentNullException>(() => pass.Use("locker number", DateTimeOffset.UtcNow));
    }

    [Test]
    public void ShouldAddDomainEventIfPassIsAlreadyNotUsable()
    {
        var pass = new GymMembershipPass
        {
            Id = "id",
            GymMembershipId = "gymMembershipId",
            UserId = "userId",
            Type = PassType.SingleUse,
            TotalUses = 1,
            RemainingUses = 0,
            ExpirationDate = null,
            GymMembership = new GymMembership
            {
                UserId = "userId",
                GymId = "gymId"
            }
        };

        pass.Use("locker number", DateTimeOffset.UtcNow);

        pass.DomainEvents.ShouldSatisfyAllConditions(
            () => pass.DomainEvents.Count.ShouldBe(1),
            () => pass.DomainEvents.First().GetType().ShouldBe(typeof(PassExpiredEvent)));
    }

    [Test]
    public void ShouldAddDomainEventIfPassBecomesExpired()
    {
        var pass = new GymMembershipPass
        {
            Id = "id",
            GymMembershipId = "gymMembershipId",
            UserId = "userId",
            Type = PassType.SingleUse,
            TotalUses = 1,
            RemainingUses = 1,
            ExpirationDate = null,
            GymMembership = new GymMembership
            {
                UserId = "userId",
                GymId = "gymId"
            }
        };

        pass.Use("locker number", DateTimeOffset.UtcNow);

        pass.DomainEvents.ShouldSatisfyAllConditions(
            () => pass.DomainEvents.Count.ShouldBe(1),
            () => pass.DomainEvents.First().GetType().ShouldBe(typeof(PassExpiredEvent)));
    }

    private DateTimeOffset? GetExpirationDate(double? expirationDaysFromNow)
    {
        return expirationDaysFromNow is null ? null : DateTimeOffset.UtcNow.AddDays((double)expirationDaysFromNow);
    }
}
