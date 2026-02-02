using System;
using FitPass.Application.FunctionalTests.Infrastructure.Testing;
using FitPass.Application.FunctionalTests.TestData;
using FitPass.Application.GymPassUsages.Queries;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using FitPass.Domain.Enums;

namespace FitPass.Application.FunctionalTests.Tests.GymPassUsageTests.Queries;

using static Testing;

public class GetGymPassUsageForMyGymTodayTests : BaseTestFixture
{
    [Test]
    public override void AuthorizeAttributeCheck()
    {
        ShouldRequireAuthorization<GetGymPassUsagesForMyGymTodayQuery>(
            Roles.GymAdministrator,
            Roles.GymStaff
        );
    }

    [Test]
    public async Task ShouldReturnGymPassUsagesForToday()
    {
        var obj = await TestEntityBuilder.BuildGymAsync();

        var now = GetUtcNow();

        var usage = new GymPassUsage
        {
            UserId = obj.gymMember.Id,
            GymId = obj.gym.Id,
            PassType = PassType.SingleUse,
            TotalPassUses = 1,
            RemainingPassUses = 0,
            PassExpirationDate = null,
            PassUseResult = PassUseResult.Success,
            LockerNumber = "2",
            PassId = "id",
            CreatedOn = now.AddDays(-1.5),
            GymSessionEndedAt = now.AddDays(-1.6),
        };

        await AddAsync(usage);

        await RunAsUserAsync(obj.gymStaff);

        var command = new GetGymPassUsagesForMyGymTodayQuery();

        var result = await SendAsync(command);

        result.Count.ShouldBe(1);
        result.Count(x => x.Id == obj.passUsage.Id).ShouldBe(1);
    }
}
