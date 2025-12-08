using FitPass.Application.FunctionalTests.TestData;
using FitPass.Application.GymMembershipPasses.Queries;
using FitPass.Domain.Constants;
using FitPass.Domain.Enums;

namespace FitPass.Application.FunctionalTests.Tests.GymMembershipPassTests.Queries;

using static Testing;

public class GetValidGymMembershipPassesTests : BaseTestFixture
{
    [Test]
    public override void AuthorizeAttributeCheck()
    {
        ShouldRequireAuthorization<GetValidGymMembershipPassesQuery>(Roles.User);
    }

    [Test]
    public async Task ShouldReturnPasses()
    {
        var gymObj1 = await TestEntityBuilder.BuildGymAsync();
        //var gymObj2 = await TestEntityBuilder.BuildGymAsync();

        await RunAsUserAsync(gymObj1.gymMember);

        var command = new GetValidGymMembershipPassesQuery();

        var result = await SendAsync(command);
        result.ShouldNotBeNull();
        result.Count.ShouldBe(2);
        result
            .FirstOrDefault(r => r.Type == PassType.SingleUse)
            .ShouldNotBeNull();

        result
            .FirstOrDefault(r => r.Type == PassType.Unlimited)
            .ShouldNotBeNull();
    }
}
