using System;
using FitPass.Application.FunctionalTests.TestData;
using FitPass.Application.GymMemberships.Queries;
using FitPass.Domain.Constants;
using FitPass.Domain.Enums;

namespace FitPass.Application.FunctionalTests.Tests.GymMembershipTests.Queries;

using static Testing;

public class GetGymMembershipsToMyGymTests : BaseTestFixture
{
    [Test]
    public override void AuthorizeAttributeCheck()
    {
        ShouldRequireAuthorization<GetGymMembershipsQueryToMyGymQuery>(Roles.GymAdministrator, Roles.GymStaff);
    }

    [Test]
    public async Task ShouldThrowIfGymEmploymentNotExists()
    {
        var gymAdmin = await ApplicationUserBuilder
            .WithRole(Roles.GymAdministrator)
            .BuildAsync();

        await RunAsUserAsync(gymAdmin);

        await Should.ThrowAsync<SystemException>(SendAsync(new GetGymMembershipsQueryToMyGymQuery(null)));
    }

    [Test]
    public async Task ShouldReturnActiveMembers()
    {
        var obj = await TestEntityBuilder.BuildGymAsync();

        var user = await ApplicationUserBuilder.BuildAsync();

        var bannedGymMembership = await GymMembershipBuilder
            .WithApplicationUserId(user.Id)
            .WithGym(obj.gym)
            .WithStatus(GymMembershipStatus.Banned)
            .BuildAsync();

        await RunAsUserAsync(obj.gymAdmin);

        var result = await SendAsync(new GetGymMembershipsQueryToMyGymQuery(GymMembershipStatus.Active));
        result.Count.ShouldBe(1);
        result.All(gm => gm.GymMembershipStatus == GymMembershipStatus.Active).ShouldBeTrue();
    }

    [Test]
    public async Task ShouldReturnBannedMembers()
    {
        var obj = await TestEntityBuilder.BuildGymAsync();

        var user = await ApplicationUserBuilder.BuildAsync();

        var bannedGymMembership = await GymMembershipBuilder
            .WithApplicationUserId(user.Id)
            .WithGym(obj.gym)
            .WithStatus(GymMembershipStatus.Banned)
            .BuildAsync();

        await RunAsUserAsync(obj.gymAdmin);

        var result = await SendAsync(new GetGymMembershipsQueryToMyGymQuery(GymMembershipStatus.Banned));
        result.Count.ShouldBe(1);
        result.All(gm => gm.GymMembershipStatus == GymMembershipStatus.Banned).ShouldBeTrue();
    }

    [Test]
    public async Task ShouldReturnAllMembers()
    {
        var obj = await TestEntityBuilder.BuildGymAsync();

        var user = await ApplicationUserBuilder.BuildAsync();

        var bannedGymMembership = await GymMembershipBuilder
            .WithApplicationUserId(user.Id)
            .WithGym(obj.gym)
            .WithStatus(GymMembershipStatus.Banned)
            .BuildAsync();

        await RunAsUserAsync(obj.gymAdmin);

        var result = await SendAsync(new GetGymMembershipsQueryToMyGymQuery(null));
        result.Count.ShouldBe(2);
        result.Where(gm => gm.GymMembershipStatus == GymMembershipStatus.Active).Count().ShouldBe(1);
        result.Where(gm => gm.GymMembershipStatus == GymMembershipStatus.Banned).Count().ShouldBe(1);
    }
}
