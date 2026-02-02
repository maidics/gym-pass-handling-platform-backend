using FitPass.Application.FunctionalTests.Infrastructure.Testing;
using FitPass.Application.FunctionalTests.TestData;
using FitPass.Application.GymMembershipPasses.Queries;
using FitPass.Domain.Constants;
using FitPass.Domain.Enums;

namespace FitPass.Application.FunctionalTests.Tests.GymMembershipPassTests.Queries;

using static Testing;

public class GetMyGymMembershipPassesTests : BaseTestFixture
{
    [Test]
    public override void AuthorizeAttributeCheck()
    {
        ShouldRequireAuthorization<GetMyGymMembershipPassesQuery>(Roles.User);
    }

    [Test]
    public async Task ShouldReturnPasses()
    {
        var obj = await TestEntityBuilder.BuildGymAsync();

        await RunAsUserAsync(obj.gymMember);

        var result = await SendAsync(new GetMyGymMembershipPassesQuery());
        result.ShouldNotBeNull();
        result.Count.ShouldBe(2);
        result.Count(x => x.Type is PassType.SingleUse or PassType.Unlimited).ShouldBe(2);
    }
}
