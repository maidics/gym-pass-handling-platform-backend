using FitPass.Application.Common.Models;
using FitPass.Application.FunctionalTests.Common.Extensions;
using FitPass.Application.FunctionalTests.Infrastructure.Testing;
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
    public async Task ShouldThrowIfParametersAreInvalid()
    {
        await RunAsAppAdminAsync();

        var command = new UpdateGymStatusCommand(string.Empty, GymStatus.Suspended, string.Empty);

        await ShouldThrowIfParametersAreInvalidAsync(command);
    }

    [Test]
    public async Task ShouldReturnNotFoundIfGymIsNotFound()
    {
        await RunAsAppAdminAsync();

        var command = new UpdateGymStatusCommand("id", GymStatus.Suspended, "rationale");

        var result = await SendAsync(command);
        result.ShouldBeFailed(ResultTypes.NotFound);
    }

    [Test]
    public async Task ShouldUpdateGymStatus()
    {
        var obj = await TestEntityBuilder.BuildGymAsync();

        await RunAsAppAdminAsync();

        var command = new UpdateGymStatusCommand(obj.gym.Id, GymStatus.Suspended, "Rationale");

        var result = await SendAsync(command);
        result.Succeeded.ShouldBeTrue();

        var updatedGym = await FindAsync<Gym>(command.GymId);
        updatedGym.ShouldNotBeNull();
        updatedGym.Status.ShouldBe(command.NewGymStatus);

        EmailFolderShouldContainEmails(2);

        await ShouldContainNotificationForUserAsync(obj.gymAdmin.Id);
        await ShouldContainNotificationForUserAsync(obj.gymStaff.Id);
    }
}
