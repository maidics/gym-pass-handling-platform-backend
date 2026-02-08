using FitPass.Application.Common.Models;
using FitPass.Application.FunctionalTests.Common.Extensions;

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
        ShouldRequireAuthorization<UpdateGymMembershipStatusCommand>(
            Roles.GymAdministrator,
            Roles.GymStaff
        );
    }

    [Test]
    public async Task ShouldThrowIfParametersAreInvalid()
    {
        await RunAsGymEmployeeAsync(Roles.GymAdministrator);

        var command = new UpdateGymMembershipStatusCommand(
            string.Empty,
            GymMembershipStatus.Banned
        );

        await ShouldThrowIfParametersAreInvalidAsync(command);
    }

    [Test]
    public async Task ShouldReturnNotFoundIfGymMembershipIsNotFound()
    {
        await RunAsGymEmployeeAsync(Roles.GymAdministrator);

        var command = new UpdateGymMembershipStatusCommand("id", GymMembershipStatus.Banned);

        var result = await SendAsync(command);
        result.ShouldBeFailed(ResultTypes.NotFound);
    }

    [Test]
    public async Task ShouldReturnNotFoundIfTheGymMembershipIsInAnotherGym()
    {
        var obj = await TestEntityBuilder.BuildGymAsync();

        await RunAsGymEmployeeAsync(Roles.GymAdministrator);

        var command = new UpdateGymMembershipStatusCommand(
            obj.gymMembership.Id,
            GymMembershipStatus.Banned
        );

        var result = await SendAsync(command);
        result.ShouldBeFailed(ResultTypes.NotFound);
    }

    [Test]
    public async Task ShouldUpdateGymMembershipStatus()
    {
        var obj = await TestEntityBuilder.BuildGymAsync();

        await Task.Delay(50);

        await RunAsUserAsync(obj.gymAdmin);

        var command = new UpdateGymMembershipStatusCommand(
            obj.gymMembership.Id,
            GymMembershipStatus.Banned
        );

        var result = await SendAsync(command);
        result.ShouldBeSuccessful();

        var updatedGymMembership = await FindAsync<GymMembership>(obj.gymMembership.Id);
        updatedGymMembership.ShouldNotBeNull();
        updatedGymMembership.Status.ShouldBe(command.NewStatus);

        EmailFolderShouldContainEmails(1);
    }
}
