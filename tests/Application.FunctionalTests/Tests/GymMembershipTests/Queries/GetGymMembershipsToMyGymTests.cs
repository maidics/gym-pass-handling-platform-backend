using FitPass.Application.FunctionalTests.TestData;
using FitPass.Application.GymMemberships.Queries;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using FitPass.Domain.Enums;

namespace FitPass.Application.FunctionalTests.Tests.GymMembershipTests.Queries;

using static Testing;

public class GetGymMembershipsToMyGymTests : BaseTestFixture
{
    [Test]
    public override void AuthorizeAttributeCheck()
    {
        ShouldRequireAuthorization<GetGymMembershipsToMyGymQuery>(Roles.GymAdministrator, Roles.GymStaff);
    }

    [Test]
    public async Task ShouldReturnActiveMembers()
    {
        var obj = await TestEntityBuilder.BuildGymAsync();

        await RunAsUserAsync(obj.gymAdmin);

        var userObj = await TestEntityBuilder.BuildDefaultUserAsync();

        await AddAsync(new GymMembership
        {
            UserId = userObj.user.Id,
            GymId = obj.gym.Id,
            Status = GymMembershipStatus.Active
        });

        var result = await SendAsync(new GetGymMembershipsToMyGymQuery(GymMembershipStatus.Active));
        result.Count.ShouldBe(2);
        result.All(gm => gm.Status == GymMembershipStatus.Active).ShouldBeTrue();
    }

    [Test]
    public async Task ShouldReturnBannedMembers()
    {
        var obj = await TestEntityBuilder.BuildGymAsync();

        var userObj1 = await TestEntityBuilder.BuildDefaultUserAsync();

        await AddAsync(new GymMembership
        {
            UserId = userObj1.user.Id,
            GymId = obj.gym.Id,
            Status = GymMembershipStatus.Banned
        });

        var userObj2 = await TestEntityBuilder.BuildDefaultUserAsync();

        await AddAsync(new GymMembership
        {
            UserId = userObj2.user.Id,
            GymId = obj.gym.Id,
            Status = GymMembershipStatus.Banned
        });

        await RunAsUserAsync(obj.gymAdmin);

        var result = await SendAsync(new GetGymMembershipsToMyGymQuery(GymMembershipStatus.Banned));
        result.Count.ShouldBe(2);
        result.All(gm => gm.Status == GymMembershipStatus.Banned).ShouldBeTrue();
    }

    [Test]
    public async Task ShouldReturnAllMembers()
    {
        var obj = await TestEntityBuilder.BuildGymAsync();

        var userObj = await TestEntityBuilder.BuildDefaultUserAsync();

        await AddAsync(new GymMembership
        {
            UserId = userObj.user.Id,
            GymId = obj.gym.Id,
            Status = GymMembershipStatus.Banned
        });

        await RunAsUserAsync(obj.gymAdmin);

        var result = await SendAsync(new GetGymMembershipsToMyGymQuery(null));
        result.Count.ShouldBe(2);
        result.Where(gm => gm.Status == GymMembershipStatus.Active).Count().ShouldBe(1);
        result.Where(gm => gm.Status == GymMembershipStatus.Banned).Count().ShouldBe(1);
    }
}
