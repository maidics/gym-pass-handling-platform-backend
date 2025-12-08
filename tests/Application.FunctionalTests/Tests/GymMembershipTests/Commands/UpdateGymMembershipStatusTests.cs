using FitPass.Application.Common.Models;
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

        await ShouldThrowIfParametersAreInvalid(new UpdateGymMembershipStatusCommand(string.Empty, GymMembershipStatus.Banned));
    }

    [Test]
    public async Task ShouldReturnNotFoundIfGymMembershipIsNotFound()
    {
        await RunAsGymEmployeeAsync(Roles.GymAdministrator);

        var command = new UpdateGymMembershipStatusCommand("non-existing-id", GymMembershipStatus.Banned);

        var result = await SendAsync(command);
        result.Type.ShouldBe(ResultTypes.NotFound);
        result.Message.ShouldContain($"{nameof(GymMembership)} not found.");
    }

    [Test]
    public async Task ShouldReturnForbiddenIfTheGymMembershipIsInAnotherGym()
    {
        var obj1 = await TestEntityBuilder.BuildGymAsync();

        var obj2 = await TestEntityBuilder.BuildGymAsync();

        await RunAsUserAsync(obj1.gymAdmin);

        var command = new UpdateGymMembershipStatusCommand(obj2.gymMembership.Id, GymMembershipStatus.Banned);

        var result = await SendAsync(command);
        result.Type.ShouldBe(ResultTypes.Forbidden);
    }

    [Test]
    public async Task ShouldUpdateGymMembershipStatus()
    {
        var obj = await TestEntityBuilder.BuildGymAsync();

        await RunAsUserAsync(obj.gymAdmin);

        var command = new UpdateGymMembershipStatusCommand(obj.gymMembership.Id, GymMembershipStatus.Banned);

        var result = await SendAsync(command);
        result.Succeeded.ShouldBeTrue();

        var updatedGymMembership = await FindAsync<GymMembership>(obj.gymMembership.Id);
        updatedGymMembership.ShouldNotBeNull();
        updatedGymMembership.Status.ShouldBe(command.NewStatus);
    }
}
