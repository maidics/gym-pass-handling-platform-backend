using System;
using FitPass.Application.Common.Exceptions;
using FitPass.Application.FunctionalTests.TestData;
using FitPass.Application.Gyms.Commands;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using FitPass.Domain.Enums;
using Microsoft.AspNetCore.Http.HttpResults;

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

        await Should.ThrowAsync<ValidationException>(SendAsync(command));
    }

    [Test]
    public async Task ShouldThrowIfGymDoesNotExists()
    {
        await RunAsAppAdminAsync();

        var command = new UpdateGymStatusCommand("invalidGymId", GymStatus.Suspended);

        await Should.ThrowAsync<NotFoundException>(SendAsync(command));
    }

    [Test]
    public async Task ShouldUpdateGymStatus()
    {
        await RunAsAppAdminAsync();

        var obj = await TestEntityBuilder.BuildGymAsync();

        var command = new UpdateGymStatusCommand(obj.gym.Id, GymStatus.Suspended);

        await SendAsync(command);

        var updatedGym = await FindAsync<Gym>(command.GymId);
        updatedGym.ShouldNotBeNull();
        updatedGym.Status.ShouldBe(command.NewGymStatus);
    }
}
