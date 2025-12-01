using FitPass.Domain.Entities;
using FitPass.Domain.Enums;
using NUnit.Framework;
using FitPass.Application.GymPassUsages.DTOs;
using Shouldly;

namespace FitPass.Application.UnitTests.Mappings;

public class GymPassUsageMapTests
{
    [Test]
    public void ShouldMapToDto()
    {
        var twoHoursAgo = DateTimeOffset.UtcNow.AddHours(-2);
        var now = DateTimeOffset.UtcNow;

        var usage = new GymPassUsage
        {
            UserId = "userId",
            GymId = "gymId",
            PassId = "passId",
            PassType = PassType.MultiUse,
            TotalPassUses = 3,
            RemainingPassUses = 2,
            PassExpirationDate = null,
            PassUseResult = PassUseResult.Success,
            LockerNumber = "locker",
            CreatedOn = twoHoursAgo,
            GymSessionEndedAt = now
        };

        var dto = usage.MapToDto();

        dto.ShouldSatisfyAllConditions(
            () => dto.UserId.ShouldBe("userId"),
            () => dto.GymId.ShouldBe("gymId"),
            () => dto.PassType.ShouldBe(PassType.MultiUse),
            () => dto.TotalPassUses.ShouldBe(3),
            () => dto.RemainingPassUses.ShouldBe(2),
            () => dto.PassExpirationDate.ShouldBeNull(),
            () => dto.PassUseResult.ShouldBe(PassUseResult.Success),
            () => dto.LockerNumber.ShouldBe("locker"),
            () => dto.CreatedOn.ShouldBe(twoHoursAgo),
            () => dto.GymSessionEndedAt.ShouldBe(now));
    }
}
