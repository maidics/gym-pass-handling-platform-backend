using System;
using FitPass.Application.Common.Exceptions;
using FitPass.Application.Common.Models;
using FitPass.Application.FunctionalTests.TestData;
using FitPass.Application.GymMembershipPasses.Commands;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using FitPass.Domain.Enums;
using FitPass.Domain.ValueObjects;

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

        var command = new GymEmployeeUseGymMembershipPassCommand(string.Empty, string.Empty, string.Empty);

        await Should.ThrowAsync<ValidationException>(SendAsync(command));
    }

    [Test]
    public async Task ShouldReturnNotFoundIfPassIsNotFound()
    {
        await RunAsGymEmployeeAsync(Roles.GymStaff);

        var command = new GymEmployeeUseGymMembershipPassCommand("invalidPassId", "2", "3");

        var result = await SendAsync(command);
        result.Type.ShouldBe(ResultTypes.NotFound);
        result.Message.ShouldNotBeEmpty();
    }

    [Test]
    public async Task ShouldReturnForbiddenIfPassBelongsToAnotherUser()
    {
        var user = await CreateUserAsync();
        var obj = await TestEntityBuilder.BuildGymAsync();

        await RunAsUserAsync(obj.gymStaff);

        var command = new GymEmployeeUseGymMembershipPassCommand(obj.singleUsePass.Id, user.Id, "test locker");

        var result = await SendAsync(command);
        result.Type.ShouldBe(ResultTypes.Forbidden);
        result.Message.ShouldNotBeEmpty();
    }

    [Test]
    public async Task ShouldReturnForbiddenIfPassBelongsToAnotherGym()
    {
        var gymObj = await TestEntityBuilder.BuildGymAsync();
        var anotherGymObj = await TestEntityBuilder.BuildGymAsync();

        await RunAsUserAsync(gymObj.gymStaff);

        var command = new GymEmployeeUseGymMembershipPassCommand(anotherGymObj.singleUsePass.Id, anotherGymObj.gymMember.Id, "test locker");
        
        var result = await SendAsync(command);
        result.Type.ShouldBe(ResultTypes.Forbidden);
        result.Message.ShouldNotBeEmpty();
    }

    [Test]
    public async Task ShouldReturnForbiddenIfUserIsBannedFromTheGym()
    {
        var obj = await TestEntityBuilder.BuildGymAsync();

        var gymMember = await CreateUserAsync();

        var bannedMembership = new GymMembership
        {
            UserId = gymMember.Id,
            GymId = obj.gym.Id,
            Status = GymMembershipStatus.Banned
        };

        var pass = GymPassProduct
                        .SingleUse(obj.gym.Id, "name", "description", true, Money.Zero("usd"))
                        .ToGymMembershipPass(bannedMembership.Id, gymMember.Id, GetUtcNow());

        await AddAsync(bannedMembership);
        await AddAsync(pass);

        await RunAsUserAsync(obj.gymStaff);

        var command = new GymEmployeeUseGymMembershipPassCommand(pass.Id, gymMember.Id, "test locker");
        
        var result = await SendAsync(command);
        result.Type.ShouldBe(ResultTypes.BusinessRuleViolation);
        result.Message.ShouldNotBeEmpty();
    }

    [TestCase(PassType.SingleUse)]
    [TestCase(PassType.MultiUse)]
    [TestCase(PassType.Unlimited)]
    public async Task ShouldReturnBusinessRuleViolationIfThePassIsNoLongerValid(PassType type)
    {
        var obj = await TestEntityBuilder.BuildGymAsync();

        var name = "Test Pass";
        var description = "Test Description";
        var price = Money.Zero("usd");
        var utcNow = GetUtcNow();

        GymMembershipPass pass;

        switch (type)
        {
            case PassType.SingleUse:
                pass = GymPassProduct.SingleUse(obj.gym.Id, name, description, true, price)
                    .ToGymMembershipPass(obj.gymMembership.Id, obj.gymMember.Id, utcNow);

                pass.RemainingUses = 0;
                break;
            case PassType.MultiUse:
                pass = GymPassProduct.MultiUse(obj.gym.Id, name, description, 10, true, price)
                    .ToGymMembershipPass(obj.gymMembership.Id, obj.gymMember.Id, utcNow);

                pass.RemainingUses = 0;
                break;
            case PassType.Unlimited:
                pass = GymPassProduct.UnlimitedUse(obj.gym.Id, name, description, 10, true, price)
                    .ToGymMembershipPass(obj.gymMembership.Id, obj.gymMember.Id, utcNow);
            
                pass.ExpirationDate = utcNow.AddYears(-1);
                break;
            default:
                throw new NotImplementedException();
        }

        await AddAsync(pass);
        
        await RunAsUserAsync(obj.gymStaff);

        var command = new GymEmployeeUseGymMembershipPassCommand(pass.Id, obj.gymMember.Id, "test locker");

        var result = await SendAsync(command);
        result.Type.ShouldBe(ResultTypes.BusinessRuleViolation);
        result.Message.ShouldNotBeEmpty();
    }

    [Test]
    public async Task ShouldUsePass()
    {
        var obj = await TestEntityBuilder.BuildGymAsync();

        var pass = GymPassProduct
                        .SingleUse(obj.gym.Id, "name", "description", true, Money.Zero("usd"))
                        .ToGymMembershipPass(obj.gymMembership.Id, obj.gymMember.Id, GetUtcNow());

        await AddAsync(pass);

        await RunAsUserAsync(obj.gymStaff);

        var command = new GymEmployeeUseGymMembershipPassCommand(pass.Id, obj.gymMember.Id, "test locker");
        
        var result = await SendAsync(command);
        result.Succeeded.ShouldBeTrue();
        result.Value.ShouldBe(PassUseResult.Success);
        
        var usageCount = await CountAsync<GymPassUsage>();
        usageCount.ShouldBe(2);
    }
}
