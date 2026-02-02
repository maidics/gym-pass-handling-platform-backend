using FitPass.Application.FunctionalTests.Common.Extensions;
using FitPass.Application.FunctionalTests.Infrastructure.Testing;
using FitPass.Application.FunctionalTests.TestData;
using FitPass.Application.Gyms.Commands;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using FitPass.Domain.Enums;
using FitPass.Domain.ValueObjects;

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
    public async Task ShouldThrowIfParametersAreInvalid()
    {
        var obj = await TestEntityBuilder.BuildGymAsync();

        await RunAsUserAsync(obj.gymAdmin);

        var command = new UpdateMyGymProfileCommand(
            string.Empty,
            new Address("line1", null, "city", null, "postalCode", "HU"),
            GymTier.Local
        );

        await ShouldThrowIfParametersAreInvalidAsync(command);
    }

    [Test]
    public async Task ShouldUpdateGymProfile()
    {
        var obj = await TestEntityBuilder.BuildGymAsync();

        await RunAsUserAsync(obj.gymAdmin);

        var command = new UpdateMyGymProfileCommand(
            "New Test Gym Name",
            new Address("line1", "line2", "city", null, "postalCode", "HU"),
            GymTier.Local
        );

        var result = await SendAsync(command);
        result.ShouldBeSuccessful();

        var updatedGym = await FindAsync<Gym>(obj.gym.Id);
        updatedGym.ShouldNotBeNull();
        updatedGym.Name.ShouldBe(command.NewName);
        updatedGym.Address.ShouldBe(command.NewAddress);
        updatedGym.Tier.ShouldBe(command.NewTier);
    }
}
