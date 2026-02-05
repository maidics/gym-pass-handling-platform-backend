using FitPass.Application.Common.Exceptions;
using FitPass.Application.Common.Models;
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

public class UpdateMyGymStatusTests : BaseTestFixture
{
    [Test]
    public override void AuthorizeAttributeCheck()
    {
        ShouldRequireAuthorization<UpdateMyGymStatusCommand>(Roles.GymAdministrator);
    }

    [Test]
    public async Task ShouldThrowIfParametersAreInvalid()
    {
        await RunAsGymEmployeeAsync(Roles.GymAdministrator);

        var command = new UpdateMyGymStatusCommand(GymStatus.Suspended);

        await ShouldThrowIfParametersAreInvalidAsync(command);
    }

    [Test]
    public async Task ShouldReturnBusinessRuleViolationIfGymIsSuspended()
    {
        var obj = await TestEntityBuilder.BuildGymAsync(GymStatus.Suspended);

        await RunAsUserAsync(obj.gymAdmin);

        var command = new UpdateMyGymStatusCommand(GymStatus.Active);

        var result = await SendAsync(command);
        result.ShouldBeFailed(ResultTypes.BusinessRuleViolation);
    }

    [Test]
    public async Task ShouldUpdateGymStatus()
    {
        var obj = await TestEntityBuilder.BuildGymAsync();

        await RunAsUserAsync(obj.gymAdmin);

        var command = new UpdateMyGymStatusCommand(GymStatus.Inactive);

        var result = await SendAsync(command);
        result.ShouldBeSuccessful();

        var gym = await FindAsync<Gym>(obj.gym.Id);
        gym.ShouldNotBeNull();
        gym.Status.ShouldBe(GymStatus.Inactive);
    }
}
