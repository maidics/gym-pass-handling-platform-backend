using System;
using FitPass.Application.Common.Exceptions;
using FitPass.Application.FunctionalTests.TestData;
using FitPass.Application.Gyms.Commands;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using FitPass.Domain.Enums;

namespace FitPass.Application.FunctionalTests.Tests.GymTests.Commands;

using static Testing;

public class UpdateMyGymProfileTests : BaseTestFixture
{
    [Test]
    public override void AuthorizeAttributeCheck()
    {
        ShouldRequireAuthorization<UpdateMyGymProfileCommand>(Roles.GymAdministrator);
    }

    [Test]
    public async Task ShouldDenyInvalidParameters()
    {
        var obj = await TestEntityBuilder.BuildGymAsync();

        await RunAsUserAsync(obj.gymAdmin);

        var command = new UpdateMyGymProfileCommand(string.Empty, string.Empty, GymTier.Local, string.Empty);

        await Should.ThrowAsync<ValidationException>(SendAsync(command));
    }

    [Test]
    public async Task ShouldUpdateGymProfile()
    {
        var obj = await TestEntityBuilder.BuildGymAsync();

        await RunAsUserAsync(obj.gymAdmin);

        var command = new UpdateMyGymProfileCommand("NewGymName", "NewGymAddress", GymTier.Elite, "NewGymOwnerName");

        await SendAsync(command);

        var updatedGym = await FindAsync<Gym>(obj.gym.Id);
        updatedGym.ShouldNotBeNull();
        updatedGym.Name.ShouldBe(command.GymName);
        updatedGym.Address.ShouldBe(command.GymAddress);
        updatedGym.Tier.ShouldBe(command.GymTier);
        updatedGym.OwnerName.ShouldBe(command.GymOwnerName);
    }
}
