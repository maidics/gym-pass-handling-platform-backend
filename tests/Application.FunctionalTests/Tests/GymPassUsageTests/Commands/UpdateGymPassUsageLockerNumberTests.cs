using System;
using FitPass.Application.Common.Exceptions;
using FitPass.Application.FunctionalTests.TestData;
using FitPass.Application.GymPassUsages.Commands;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;

namespace FitPass.Application.FunctionalTests.Tests.GymPassUsageTests.Commands;

using static Testing;

public class UpdateGymPassUsageLockerNumberTests : BaseTestFixture
{
    [Test]
    public override void AuthorizeAttributeCheck()
    {
        ShouldRequireAuthorization<UpdateGymPassUsageLockerNumberCommand>(Roles.GymAdministrator, Roles.GymStaff);
    }

    [Test]
    public async Task ShouldDenyInvalidParameters()
    {
        await RunAsGymEmployeeAsync(Roles.GymAdministrator);

        var command = new UpdateGymPassUsageLockerNumberCommand(string.Empty, string.Empty);

        await ShouldThrowIfParametersAreInvalid(command);
    }

    [Test]
    public async Task ShouldThrowIfNotExists()
    {
        await RunAsGymEmployeeAsync(Roles.GymStaff);

        var command = new UpdateGymPassUsageLockerNumberCommand("invalidGymPassUsageId", "20");

        await ShouldThrowIfNotFound(command);
    }

    [Test]
    public async Task ShouldThrowIfGymSessionAlreadyEnded()
    {
        var obj = await TestEntityBuilder.BuildGymAsync();

        var gymPassUsage = await GymPassUsageBuilder
            .WithApplicationUserId(obj.gymMember.Id)
            .WithGymId(obj.gym.Id)
            .WithPass(obj.singleUsePass)
            .WithGymSessionFinishedAt(DateTimeOffset.UtcNow)
            .WithLockerNumber("19")
            .BuildAsync();

        await RunAsUserAsync(obj.gymStaff);

        var command = new UpdateGymPassUsageLockerNumberCommand(gymPassUsage.Id, "20");

        await Should.ThrowAsync<BadRequestException>(SendAsync(command));
    }

    [Test]
    public async Task ShouldUpdateGymSessionEndedAt()
    {
        var obj = await TestEntityBuilder.BuildGymAsync();

        var gymPassUsage = await GymPassUsageBuilder
            .WithApplicationUserId(obj.gymMember.Id)
            .WithGymId(obj.gym.Id)
            .WithPass(obj.singleUsePass)
            .WithLockerNumber("19")
            .BuildAsync();

        await RunAsUserAsync(obj.gymStaff);

        string newLockerNumber = "20";

        var command = new UpdateGymPassUsageLockerNumberCommand(gymPassUsage.Id, newLockerNumber);

        await SendAsync(command);

        var updatedGymPassUsage = await FindAsync<GymPassUsage>(gymPassUsage.Id);
        updatedGymPassUsage.ShouldNotBeNull();
        updatedGymPassUsage.LockerNumber.ShouldBe(newLockerNumber);
    }
}
