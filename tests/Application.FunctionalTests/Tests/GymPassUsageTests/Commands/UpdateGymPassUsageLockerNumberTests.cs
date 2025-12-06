using FitPass.Application.Common.Models;
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
    public async Task ShouldReturnNotFoundIfNotFound()
    {
        await RunAsGymEmployeeAsync(Roles.GymStaff);

        var command = new UpdateGymPassUsageLockerNumberCommand("invalidGymPassUsageId", "20");

        var result = await SendAsync(command);
        result.Type.ShouldBe(ResultTypes.NotFound);
        result.Message.ShouldContain($"{nameof(GymPassUsage)} not found");
    }

    [Test]
    public async Task ShouldReturnBusinessRuleViolationIfGymSessionAlreadyEnded()
    {
        var obj = await TestEntityBuilder.BuildGymAsync();

        var usage = obj.singleUsePass.Use(obj.gym.Id, "test locker", GetUtcNow());
        usage.EndGymSession(GetUtcNow());

        await AddAsync(usage);

        await RunAsUserAsync(obj.gymStaff);

        var command = new UpdateGymPassUsageLockerNumberCommand(usage.Id, "20");

        var result = await SendAsync(command);
        result.Type.ShouldBe(ResultTypes.BusinessRuleViolation);
        result.Message.ShouldContain("Gym session already ended, you cannot change the locker number after this");
    }

    [Test]
    public async Task ShouldUpdateLockerNumber()
    {
        var obj = await TestEntityBuilder.BuildGymAsync();

        var usage = obj.singleUsePass.Use(obj.gym.Id, "test locker", GetUtcNow());

        await AddAsync(usage);

        await RunAsUserAsync(obj.gymStaff);

        string newLockerNumber = "new test locker";

        var command = new UpdateGymPassUsageLockerNumberCommand(usage.Id, newLockerNumber);

        var result = await SendAsync(command);
        result.Succeeded.ShouldBeTrue();

        var updatedGymPassUsage = await FindAsync<GymPassUsage>(usage.Id);
        updatedGymPassUsage.ShouldNotBeNull();
        updatedGymPassUsage.LockerNumber.ShouldBe(newLockerNumber);
    }
}
