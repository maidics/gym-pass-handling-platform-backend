using FitPass.Domain.Enums;
using NUnit.Framework;
using Shouldly;

namespace FitPass.Domain.UnitTests.Entities;

public class GymPassUsageTests
{
    [Test]
    public void ShouldFinishGymSession()
    {
        var utcNow = DateTimeOffset.UtcNow;

        var usage = new Domain.Entities.GymPassUsage
        {
            UserId = "user1",
            GymId = "gym1",
            PassType = PassType.SingleUse,
            TotalPassUses = 1,
            RemainingPassUses = 0,
            PassExpirationDate = null,
            PassUseResult = PassUseResult.Success,
            LockerNumber = "L123",
            PassId = "pass1",
            CreatedOn = utcNow
        };

        usage = usage.FinishGymSession(utcNow.AddHours(1));

        usage.GymSessionEndedAt.ShouldNotBeNull();
        usage.GymSessionEndedAt.ShouldBe(utcNow.AddHours(1));

        var length = usage.GymSessionLengthToTimeSpan();

        length.ShouldBe(TimeSpan.FromHours(1));
    }

    [Test]
    public void ShouldThrowWhenFinishingUnsuccessfulGymSession()
    {
        var utcNow = DateTimeOffset.UtcNow;

        var usage = new Domain.Entities.GymPassUsage
        {
            UserId = "user1",
            GymId = "gym1",
            PassType = PassType.SingleUse,
            TotalPassUses = 1,
            RemainingPassUses = 0,
            PassExpirationDate = null,
            PassUseResult = PassUseResult.Expired,
            LockerNumber = "L123",
            PassId = "pass1",
            CreatedOn = utcNow
        };

        Should.Throw<InvalidOperationException>(() => usage.FinishGymSession(utcNow.AddHours(1)));
    }

    [Test]
    public void ShouldCheckIfGymSessionEnded()
    {
        var utcNow = DateTimeOffset.UtcNow;
        var usage = new Domain.Entities.GymPassUsage
        {
            UserId = "user1",
            GymId = "gym1",
            PassType = PassType.SingleUse,
            TotalPassUses = 1,
            RemainingPassUses = 0,
            PassExpirationDate = null,
            PassUseResult = PassUseResult.Success,
            LockerNumber = "L123",
            PassId = "pass1",
            CreatedOn = utcNow
        };
        usage.HasGymSessionEnded().ShouldBeFalse();
        usage.GymSessionEndedAt = utcNow.AddHours(1);
        usage.HasGymSessionEnded().ShouldBeTrue();
    }
}
