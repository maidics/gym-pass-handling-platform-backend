using FitPass.Application.Common.Models;
using FitPass.Application.FunctionalTests.Common.Extensions;
using FitPass.Application.FunctionalTests.Infrastructure.Testing;
using FitPass.Application.FunctionalTests.TestData;
using FitPass.Application.GymPassUsages.Commands;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using FitPass.Domain.Enums;

namespace FitPass.Application.FunctionalTests.Tests.GymPassUsageTests.Commands;

using static Testing;

public class EndUserGymSessionTests : BaseTestFixture
{
    [Test]
    public override void AuthorizeAttributeCheck()
    {
        ShouldRequireAuthorization<EndUserGymSessionCommand>(
            Roles.GymAdministrator,
            Roles.GymStaff
        );
    }

    [Test]
    public async Task ShouldThrowIfParametersAreInvalid()
    {
        await RunAsGymEmployeeAsync(Roles.GymAdministrator);

        await ShouldThrowIfParametersAreInvalidAsync(new EndUserGymSessionCommand(string.Empty));
    }

    [Test]
    public async Task ShouldReturnNotFoundIfGymPassUsageIsNotFound()
    {
        await RunAsGymEmployeeAsync(Roles.GymAdministrator);

        var command = new EndUserGymSessionCommand("id");

        var result = await SendAsync(command);
        result.ShouldBeFailed(ResultTypes.NotFound);
    }

    [Test]
    public async Task ShouldEndUserGymSession()
    {
        var obj = await TestEntityBuilder.BuildGymAsync();

        var usage = new GymPassUsage
        {
            UserId = obj.gymMember.Id,
            GymId = obj.gym.Id,
            PassId = obj.singleUsePass.Id,
            TotalPassUses = 1,
            RemainingPassUses = 0,
            CreatedOn = DateTime.UtcNow.AddHours(-1),
            PassType = obj.singleUsePass.Type,
            PassExpirationDate = obj.singleUsePass.ExpirationDate,
            PassUseResult = PassUseResult.Success,
            LockerNumber = "2",
        };

        await AddAsync(usage);

        await RunAsUserAsync(obj.gymStaff);

        var command = new EndUserGymSessionCommand(usage.Id);

        var result = await SendAsync(command);
        result.ShouldBeSuccessful();

        var updatedGymPassUsage = await FindAsync<GymPassUsage>(usage.Id);
        updatedGymPassUsage.ShouldNotBeNull();
        updatedGymPassUsage.Id.ShouldBe(usage.Id);
        updatedGymPassUsage.GymSessionEndedAt.ShouldNotBeNull();

        await ShouldContainNotificationForUserAsync(usage.UserId);
    }
}
