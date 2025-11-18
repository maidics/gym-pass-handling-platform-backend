using System;
using FitPass.Application.FunctionalTests.TestData;
using FitPass.Application.GymPassUsages.Queries;
using FitPass.Domain.Constants;

namespace FitPass.Application.FunctionalTests.Tests.GymPassUsageTests.Queries;

using static Testing;

public class GetGymPassUsageForMyGymTodayTests : BaseTestFixture
{
    [Test]
    public override void AuthorizeAttributeCheck()
    {
        ShouldRequireAuthorization<GetGymPassUsagesForMyGymTodayQuery>(Roles.GymAdministrator, Roles.GymStaff);
    }

    [Test]
    public async Task ShouldReturnGymPassUsagesForToday()
    {
        var obj1 = await TestEntityBuilder.BuildGymAsync();
        var obj2 = await TestEntityBuilder.BuildGymAsync();

        var gymPassUsageToday = await GymPassUsageBuilder
            .WithApplicationUserId(obj1.gymMember.Id)
            .WithGymId(obj1.gym.Id)
            .WithPass(obj1.singleUsePass)
            .WithLockerNumber("19")
            .BuildAsync();

        var gymPassUsageNotToday = await GymPassUsageBuilder
            .WithApplicationUserId(obj1.gymMember.Id)
            .WithGymId(obj1.gym.Id)
            .WithPass(obj1.singleUsePass)
            .WithLockerNumber("19")
            .WithCreatedOn(DateTimeOffset.UtcNow.AddDays(-1))
            .BuildAsync();

        var gymPassUsageInAnotherGym = await GymPassUsageBuilder
            .WithApplicationUserId(obj2.gymMember.Id)
            .WithGymId(obj2.gym.Id)
            .WithPass(obj2.singleUsePass)
            .WithLockerNumber("19")
            .BuildAsync();

        await RunAsUserAsync(obj1.gymStaff);

        var command = new GetGymPassUsagesForMyGymTodayQuery();

        var result = await SendAsync(command);

        result.Count.ShouldBe(1);
        result.First().AssertTo(gymPassUsageToday);
    }
}
