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

        await RunAsUserAsync(obj1.gymStaff);

        var command = new GetGymPassUsagesForMyGymTodayQuery();

        var result = await SendAsync(command);

        result.Count.ShouldBe(1);
        result.First().AssertTo(obj1.passUsage);
    }
}
