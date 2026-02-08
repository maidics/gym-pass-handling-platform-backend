using FitPass.Application.Common.Exceptions;
using FitPass.Application.Common.Models;
using FitPass.Application.FunctionalTests.Common.Extensions;

using FitPass.Application.Users.Commands.RoleHandling;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;

namespace FitPass.Application.FunctionalTests.Tests.UserTests.Commands.RoleHandling;

using static Testing;

public class PromotePendingGymEmployeeToGymStaffRoleTests : BaseTestFixture
{
    [Test]
    public override void AuthorizeAttributeCheck()
    {
        ShouldRequireAuthorization<PromotePendingGymEmployeeToGymStaffRoleCommand>(
            Roles.GymAdministrator
        );
    }

    [TestCase("")]
    [TestCase("invalid@email")]
    public async Task ShouldThrowIfParametersAreInvalid(string pendingGymEmployeeEmail)
    {
        await RunAsGymEmployeeAsync(Roles.GymAdministrator);

        var command = new PromotePendingGymEmployeeToGymStaffRoleCommand(pendingGymEmployeeEmail);

        await ShouldThrowIfParametersAreInvalidAsync(command);
    }

    [Test]
    public async Task ShouldReturnNotFoundIfUserNotFound()
    {
        await RunAsGymEmployeeAsync(Roles.GymAdministrator);

        var command = new PromotePendingGymEmployeeToGymStaffRoleCommand("non-existing-user-id");

        var result = await SendAsync(command);
        result.ShouldBeFailed(ResultTypes.NotFound);
    }

    [Test]
    public async Task ShouldReturnForbiddenIfUserIsNotPendingGymEmployee()
    {
        var user = await CreateUserAsync();

        await RunAsGymEmployeeAsync(Roles.GymAdministrator);

        var command = new PromotePendingGymEmployeeToGymStaffRoleCommand(user.Id);

        var result = await SendAsync(command);
        result.ShouldBeFailed(ResultTypes.Forbidden);
    }

    [Test]
    public async Task ShouldPromotePendingGymEmployee()
    {
        var gymAdminObj = await RunAsGymEmployeeAsync(Roles.GymAdministrator);

        var pendingGymEmployee = await CreateUserAsync(role: Roles.PendingGymEmployee);

        var command = new PromotePendingGymEmployeeToGymStaffRoleCommand(pendingGymEmployee.Id);

        var result = await SendAsync(command);
        result.ShouldBeSuccessful();

        var rolesAfterPromotion = await GetUserRolesAsync(pendingGymEmployee.Id);

        rolesAfterPromotion.Count.ShouldBe(1);
        rolesAfterPromotion.First().ShouldBe(Roles.GymStaff);

        var gymStaffEmployment = await FindByUserIdAsync<GymEmployment>(pendingGymEmployee.Id);

        gymStaffEmployment.ShouldNotBeNull();
        gymStaffEmployment.GymId.ShouldBe(gymAdminObj.gym.Id);
        gymStaffEmployment.UserId.ShouldBe(pendingGymEmployee.Id);
    }
}
