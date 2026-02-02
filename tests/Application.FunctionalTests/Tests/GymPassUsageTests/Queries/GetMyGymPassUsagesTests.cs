using FitPass.Application.FunctionalTests.Infrastructure.Testing;
using FitPass.Application.FunctionalTests.TestData;
using FitPass.Application.GymPassUsages.Queries;
using FitPass.Domain.Constants;

namespace FitPass.Application.FunctionalTests.Tests.GymPassUsageTests.Queries;

using static Testing;

public class GetMyGymPassUsagesTests : BaseTestFixture
{
    [Test]
    public override void AuthorizeAttributeCheck()
    {
        ShouldRequireAuthorization<GetMyGymPassUsagesQuery>(Roles.User);
    }

    [Test]
    public async Task ShouldReturnGymPassUsages()
    {
        var obj = await TestEntityBuilder.BuildGymAsync();

        await RunAsUserAsync(obj.gymMember);

        var result = await SendAsync(new GetMyGymPassUsagesQuery());

        result.Count.ShouldBe(1);
        result.Count(x => x.Id == obj.passUsage.Id).ShouldBe(1);
    }
}
