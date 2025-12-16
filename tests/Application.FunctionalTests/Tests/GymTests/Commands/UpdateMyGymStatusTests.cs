using FitPass.Application.Common.Exceptions;
using FitPass.Application.Common.Models;
using FitPass.Application.FunctionalTests.TestData;
using FitPass.Application.Gyms.Commands;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using FitPass.Domain.Enums;
using FitPass.Domain.ValueObjects;

namespace FitPass.Application.FunctionalTests.Tests.GymTests.Commands;

using static Testing;

public class UpdateMyGymStatusTests : BaseTestFixture
{
    [Test]
    public override void AuthorizeAttributeCheck()
    {
        ShouldRequireAuthorization<UpdateMyGymStatusCommand>(Roles.GymAdministrator);
    }

    [Test]
    public async Task ShouldThrowIfNewGymStatusIsSuspended()
    {
        await RunAsGymEmployeeAsync(Roles.GymAdministrator);

        var command = new UpdateMyGymStatusCommand(GymStatus.Suspended);

        await Should.ThrowAsync<ValidationException>(() => SendAsync(command));
    }

    [Test]
    public async Task ShouldReturnBusinessRuleViolationIfGymIsSuspended() 
    {
        var gymAdmin = await CreateUserAsync(role: Roles.GymAdministrator);

        var gym = new Gym
        {
            Name = "Test Gym",
            Status = GymStatus.Suspended,
            Address = new Address("lin1", "line2", "city", null, "postalCode", "HU"),
            Tier = GymTier.Local
        };

        await AddAsync(gym);

        var gymEmployment = new GymEmployment
        {
            UserId = gymAdmin.Id,
            GymId = gym.Id,
            Role = Roles.GymAdministrator,
            EmploymentStart = GetUtcNow()
        };

        await AddAsync(gymEmployment);

        await RunAsUserAsync(gymAdmin);

        var command = new UpdateMyGymStatusCommand(GymStatus.Active);

        var result = await SendAsync(command);
        result.Type.ShouldBe(ResultTypes.BusinessRuleViolation);
        result.Message.ShouldNotBeEmpty();
    }

    [Test]
    public async Task ShouldUpdateGymStatus()
    {
        var obj = await TestEntityBuilder.BuildGymAsync();

        await RunAsUserAsync(obj.gymAdmin);

        var command = new UpdateMyGymStatusCommand(GymStatus.Inactive);

        var result = await SendAsync(command);
        result.Succeeded.ShouldBeTrue();

        var gym = await FindAsync<Gym>(obj.gym.Id);
        gym.ShouldNotBeNull();
        gym.Status.ShouldBe(GymStatus.Inactive);
    }
}
