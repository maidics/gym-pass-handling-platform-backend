using System;
using FitPass.Application.Common.Exceptions;
using FitPass.Application.FunctionalTests.TestData;
using FitPass.Application.GymMembershipPasses.Commands;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using FitPass.Domain.Enums;

namespace FitPass.Application.FunctionalTests.Tests.GymMembershipPassTests.Commands;

using static Testing;

public class GymEmployeeUseGymMembershipPassTests : BaseTestFixture
{
    [Test]
    public override void AuthorizeAttributeCheck()
    {
        ShouldRequireAuthorization<GymEmployeeUseGymMembershipPassCommand>(Roles.GymAdministrator, Roles.GymStaff);
    }

    [Test]
    public async Task ShouldDenyInvalidParameters()
    {
        await RunAsGymEmployeeAsync(Roles.GymAdministrator);

        var command = new GymEmployeeUseGymMembershipPassCommand(string.Empty, string.Empty);

        await Should.ThrowAsync<ValidationException>(SendAsync(command));
    }

    [Test]
    public async Task ShouldThrowIfPassIsNotFound()
    {
        await RunAsGymEmployeeAsync(Roles.GymStaff);

        var command = new GymEmployeeUseGymMembershipPassCommand("invalidPassId", "2");

        await Should.ThrowAsync<NotFoundException>(SendAsync(command));
    }

    [Test]
    public async Task ShouldThrowIfPassIsForAnotherGym()
    {
        var obj = await TestEntityBuilder.BuildGymAsync();

        await RunAsGymEmployeeAsync(Roles.GymStaff);

        var command = new GymEmployeeUseGymMembershipPassCommand(obj.singleUsePass.Id, "30");

        await Should.ThrowAsync<ForbiddenAccessException>(SendAsync(command));
    }

    [Test]
    public async Task ShouldReturnAlreadyHasNoUsesLeftAndPassShouldBeDeleted()
    {
        var obj = await TestEntityBuilder.BuildGymAsync();

        await RunAsUserAsync(obj.gymAdmin);

        string lockerNumber = "20";

        var command = new GymEmployeeUseGymMembershipPassCommand(obj.noUsePass.Id, lockerNumber);

        var result = await SendAsync(command);

        result.ShouldBe(PassUseResult.AlreadyHasNoUsesLeft);

        var noUsePass = await FindAsync<GymMembershipPass>(obj.noUsePass.Id);
        noUsePass.ShouldBeNull();

        var gymPassUsageCount = await CountAsync<GymPassUsage>();
        gymPassUsageCount.ShouldBe(1);

        var gymPassUsage = await GetFirstAsync<GymPassUsage>();
        gymPassUsage.ShouldNotBeNull();
        gymPassUsage.AssertTo(obj.gymMember.Id, obj.gym.Id, obj.noUsePass, result, lockerNumber);
    }

    [Test]
    public async Task ShouldReturnSuccessAndDeletePass()
    {
        var obj = await TestEntityBuilder.BuildGymAsync();

        await RunAsUserAsync(obj.gymAdmin);

        string lockerNumber = "20";

        var command = new GymEmployeeUseGymMembershipPassCommand(obj.singleUsePass.Id, lockerNumber);

        var result = await SendAsync(command);

        result.ShouldBe(PassUseResult.Success);

        var usedPass = await FindAsync<GymMembershipPass>(obj.singleUsePass.Id);
        usedPass.ShouldBeNull();

        var gymPassUsageCount = await CountAsync<GymPassUsage>();
        gymPassUsageCount.ShouldBe(1);

        var gymPassUsage = await GetFirstAsync<GymPassUsage>();
        gymPassUsage.ShouldNotBeNull();
        gymPassUsage.AssertTo(obj.gymMember.Id, obj.gym.Id, obj.singleUsePass, result, lockerNumber);
    }

    [Test]
    public async Task ShouldReturnUnlimitedPassExpired()
    {
        var obj = await TestEntityBuilder.BuildGymAsync();

        await RunAsUserAsync(obj.gymAdmin);

        string lockerNumber = "20";

        var command = new GymEmployeeUseGymMembershipPassCommand(obj.expiredPass.Id, lockerNumber);

        var result = await SendAsync(command);

        result.ShouldBe(PassUseResult.UnlimitedPassAlreadyExpired);

        var usedPass = await FindAsync<GymMembershipPass>(obj.expiredPass.Id);
        usedPass.ShouldBeNull();

        var gymPassUsageCount = await CountAsync<GymPassUsage>();
        gymPassUsageCount.ShouldBe(1);

        var gymPassUsage = await GetFirstAsync<GymPassUsage>();
        gymPassUsage.ShouldNotBeNull();
        gymPassUsage.AssertTo(obj.gymMember.Id, obj.gym.Id, obj.expiredPass, result, lockerNumber);
    }
}
