using FitPass.Application.Common.Models;
using FitPass.Application.FunctionalTests.TestData;
using FitPass.Application.Gyms.Commands;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using FitPass.Domain.Enums;

namespace FitPass.Application.FunctionalTests.Tests.GymTests.Commands;

using static Testing;

public class UpdateGymStatusTests : BaseTestFixture
{
    [Test]
    public override void AuthorizeAttributeCheck()
    {
        ShouldRequireAuthorization<UpdateGymStatusCommand>(Roles.AppAdministrator);
    }

    [Test]
    public async Task ShouldDenyInvalidParameters()
    {
        await RunAsAppAdminAsync();

        var command = new UpdateGymStatusCommand(string.Empty, GymStatus.Suspended);

        await ShouldThrowIfParametersAreInvalid(command);
    }

    [Test]
    public async Task ShouldReturnNotFoundIfGymIsNotFound()
    {
        await RunAsAppAdminAsync();

        var command = new UpdateGymStatusCommand("invalidGymId", GymStatus.Suspended);

        var result = await SendAsync(command);
        result.Type.ShouldBe(ResultTypes.NotFound);
        result.Message.ShouldBe("Gym not found.");
    }

    [Test]
    public async Task ShouldUpdateGymStatus()
    {
        var obj = await TestEntityBuilder.BuildGymAsync();

        await RunAsAppAdminAsync();

        var command = new UpdateGymStatusCommand(obj.gym.Id, GymStatus.Suspended);

        var result = await SendAsync(command);
        result.Succeeded.ShouldBeTrue();

        var updatedGym = await FindAsync<Gym>(command.GymId);
        updatedGym.ShouldNotBeNull();
        updatedGym.Status.ShouldBe(command.NewGymStatus);
    }
}
