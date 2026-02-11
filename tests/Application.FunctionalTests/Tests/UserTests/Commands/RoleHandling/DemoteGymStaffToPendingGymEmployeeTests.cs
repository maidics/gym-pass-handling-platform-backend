using FitPass.Application.Common.Exceptions;
using FitPass.Application.Common.Models;
using FitPass.Application.FunctionalTests.Common.Extensions;
using FitPass.Application.FunctionalTests.TestData;
using FitPass.Application.Users.Commands.RoleHandling;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;

namespace FitPass.Application.FunctionalTests.Tests.UserTests.Commands.RoleHandling;

using static Testing;

public class DemoteGymStaffToPendingGymEmployeeTests : BaseTestFixture
{
    [Test]
    public override void AuthorizeAttributeCheck()
    {
        ShouldRequireAuthorization<DemoteGymStaffToPendingGymEmployeeCommand>(
            Roles.GymAdministrator
        );
    }

    [Test]
    public async Task ShouldThrowIfParametersAreInvalid()
    {
        await RunAsGymEmployeeAsync(Roles.GymAdministrator);

        var command = new DemoteGymStaffToPendingGymEmployeeCommand(string.Empty);

        await ShouldThrowIfParametersAreInvalidAsync(command);
    }

    [Test]
    public async Task ShouldDenyDemotionToNonGymStaffUser()
    {
        var obj = await TestEntityBuilder.BuildGymAsync();

        await RunAsGymEmployeeAsync(Roles.GymAdministrator);

        var command = new DemoteGymStaffToPendingGymEmployeeCommand(obj.gymAdmin.Id);

        var result = await SendAsync(command);
        result.ShouldBeFailed(ResultTypes.Forbidden);
    }

    [Test]
    public async Task ShouldDenyDemotionToGymStaffThatIsInAnotherGym()
    {
        var obj = await TestEntityBuilder.BuildGymAsync();

        await RunAsGymEmployeeAsync(Roles.GymAdministrator);

        var command = new DemoteGymStaffToPendingGymEmployeeCommand(obj.gymStaff.Id);

        var result = await SendAsync(command);
        result.ShouldBeFailed(ResultTypes.Forbidden);
    }

    [Test]
    public async Task ShouldDenyDemotionToAnotherGymAdmin()
    {
        var obj = await TestEntityBuilder.BuildGymAsync();

        var gymAdmin2 = await CreateUserAsync(role: Roles.GymAdministrator);

        await AddAsync(
            new GymEmployment
            {
                UserId = gymAdmin2.Id,
                GymId = obj.gym.Id,
                Role = Roles.GymAdministrator,
                CreatedOn = GetUtcNow(),
            }
        );

        await RunAsUserAsync(obj.gymAdmin);

        var command = new DemoteGymStaffToPendingGymEmployeeCommand(gymAdmin2.Id);

        var result = await SendAsync(command);
        result.ShouldBeFailed(ResultTypes.Forbidden);
    }

    [Test]
    public async Task ShouldDemoteGymStaff()
    {
        var obj = await TestEntityBuilder.BuildGymAsync();

        await RunAsUserAsync(obj.gymAdmin);

        var command = new DemoteGymStaffToPendingGymEmployeeCommand(obj.gymStaff.Id);

        var result = await SendAsync(command);
        result.ShouldBeSuccessful();

        var rolesAfterDemotion = await GetUserRolesAsync(obj.gymStaff.Id);

        rolesAfterDemotion.Count.ShouldBe(1);
        rolesAfterDemotion.First().ShouldBe(Roles.PendingGymEmployee);

        var gymStaffGymEmployment = await FindAsync<GymEmployment>(obj.gymStaffGymEmployment.Id);
        gymStaffGymEmployment.ShouldBeNull();
    }
}
