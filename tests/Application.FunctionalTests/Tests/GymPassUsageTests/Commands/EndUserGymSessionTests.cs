using FitPass.Application.FunctionalTests.TestData;
using FitPass.Application.GymPassUsages.Commands;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;

namespace FitPass.Application.FunctionalTests.Tests.GymPassUsageTests.Commands;

using static Testing;

public class EndUserGymSessionTests : BaseTestFixture
{
    [Test]
    public override void AuthorizeAttributeCheck()
    {
        ShouldRequireAuthorization<EndUserGymSessionCommand>(Roles.GymAdministrator, Roles.GymStaff);
    }

    [Test]
    public async Task ShouldDenyInvalidParameters()
    {
        await RunAsGymEmployeeAsync(Roles.GymAdministrator);

        var command = new EndUserGymSessionCommand(string.Empty);

        await ShouldThrowIfParametersAreInvalid(command);
    }

    [Test]
    public async Task ShouldIfGymPassUsageNotExists()
    {
        await RunAsGymEmployeeAsync(Roles.GymAdministrator);

        var command = new EndUserGymSessionCommand("invalidGymPassUsageId");

        await ShouldThrowIfParametersAreInvalid(command);
    }

    [Test]
    public async Task ShouldEndUserGymSession()
    {
        var obj = await TestEntityBuilder.BuildGymAsync();

        var gymPassUsage = await GymPassUsageBuilder
            .WithApplicationUserId(obj.gymMember.Id)
            .WithGymId(obj.gym.Id)
            .WithPass(obj.singleUsePass)
            .WithLockerNumber("19")
            .BuildAsync();

        await RunAsUserAsync(obj.gymStaff);

        var command = new EndUserGymSessionCommand(gymPassUsage.Id);

        await SendAsync(command);

        var updatedGymPassUsage = await FindAsync<GymPassUsage>(gymPassUsage.Id);
        updatedGymPassUsage.ShouldNotBeNull();
        updatedGymPassUsage.GymSessionEndedAt.ShouldNotBeNull();
    }
}
