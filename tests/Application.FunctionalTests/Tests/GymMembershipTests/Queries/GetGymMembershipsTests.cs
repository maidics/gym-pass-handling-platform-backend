using FitPass.Application.FunctionalTests.Infrastructure.Testing;
using FitPass.Application.FunctionalTests.TestData;
using FitPass.Application.GymMemberships.Queries;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using FitPass.Domain.Enums;

namespace FitPass.Application.FunctionalTests.Tests.GymMembershipTests.Queries;

using static Testing;

public class GetGymMembershipsTests : BaseTestFixture
{
    [Test]
    public override void AuthorizeAttributeCheck()
    {
        ShouldRequireAuthorization<GetGymMembershipsToMyGymQuery>(
            Roles.GymAdministrator,
            Roles.GymStaff
        );
    }

    [Test]
    public async Task ShouldReturnGymMembershipsWithUserProfileAndEmail()
    {
        var obj = await TestEntityBuilder.BuildGymAsync();

        var userObj = await TestEntityBuilder.BuildDefaultUserAsync();

        await AddAsync(
            new GymMembership
            {
                UserId = userObj.user.Id,
                GymId = obj.gym.Id,
                Status = GymMembershipStatus.Banned,
            }
        );

        await RunAsUserAsync(obj.gymAdmin);

        var result = await SendAsync(new GetGymMembershipsToMyGymQuery());
        result.Count.ShouldBe(2);
    }
}
