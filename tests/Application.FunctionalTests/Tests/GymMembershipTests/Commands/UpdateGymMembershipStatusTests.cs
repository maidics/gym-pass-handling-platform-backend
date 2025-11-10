using System;
using FitPass.Application.Common.Exceptions;
using FitPass.Application.FunctionalTests.TestData;
using FitPass.Application.GymMemberships.Commands;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using FitPass.Domain.Enums;

namespace FitPass.Application.FunctionalTests.Tests.GymMembershipTests.Commands;

using static Testing;

public class UpdateGymMembershipStatusTests : BaseTestFixture
{
    [Test]
    public override void AuthorizeAttributeCheck()
    {
        ShouldRequireAuthorization<UpdateGymMembershipStatusCommand>(Roles.GymAdministrator, Roles.GymStaff);
    }

    [Test]
    public async Task ShouldDenyInvalidParameters()
    {
        await RunAsGymEmployeeAsync(Roles.GymAdministrator);

        await Should.ThrowAsync<ValidationException>(SendAsync(new UpdateGymMembershipStatusCommand(string.Empty, GymMembershipStatus.Banned)));
    }

    [Test]
    public async Task ShouldThrowIfGymMembershipNotExists()
    {
        await RunAsGymEmployeeAsync(Roles.GymAdministrator);

        await Should.ThrowAsync<NotFoundException>(SendAsync(new UpdateGymMembershipStatusCommand("invalidGymMembershipId", GymMembershipStatus.Banned)));
    }

    [Test]
    public async Task ShouldThrowIfGymMembershipIsInAnotherGym()
    {
        await RunAsGymEmployeeAsync(Roles.GymAdministrator);

        var obj = await TestEntityBuilder.BuildDefaultUserAsync();

        var anotherGym = await GymBuilder.BuildAsync();

        var gymMembership = await GymMembershipBuilder
            .WithApplicationUserId(obj.user.Id)
            .WithGym(anotherGym)
            .BuildAsync();

        var command = new UpdateGymMembershipStatusCommand(gymMembership.Id, GymMembershipStatus.Banned);

        await Should.ThrowAsync<ForbiddenAccessException>(SendAsync(command));
    }

    [Test]
    public async Task ShouldUpdateGymMembershipStatus()
    {
        var gymAdminObj = await RunAsGymEmployeeAsync(Roles.GymAdministrator);

        var obj = await TestEntityBuilder.BuildDefaultUserAsync();

        var gymMembership = await GymMembershipBuilder
            .WithApplicationUserId(obj.user.Id)
            .WithGym(gymAdminObj.gym)
            .BuildAsync();

        var command = new UpdateGymMembershipStatusCommand(gymMembership.Id, GymMembershipStatus.Banned);

        await SendAsync(command);

        var updatedGymMembership = await FindAsync<GymMembership>(gymMembership.Id);
        updatedGymMembership.ShouldNotBeNull();
        updatedGymMembership.Status.ShouldBe(command.NewStatus);
    }
}
