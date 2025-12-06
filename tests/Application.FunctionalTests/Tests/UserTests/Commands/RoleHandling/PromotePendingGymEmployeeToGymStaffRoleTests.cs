using FitPass.Application.Common.Exceptions;
using FitPass.Application.Common.Models;
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
        ShouldRequireAuthorization<PromotePendingGymEmployeeToGymStaffRoleCommand>(Roles.GymAdministrator);
    }

    [Test]
    public async Task ShouldDenyInvalidUserId()
    {
        await RunAsGymEmployeeAsync(Roles.GymAdministrator);

        var command = new PromotePendingGymEmployeeToGymStaffRoleCommand(string.Empty);

        await Should.ThrowAsync<ValidationException>(() => SendAsync(command));
    }

    [Test]
    public async Task ShouldReturnNotFoundIfUserNotFound()
    {
        await RunAsGymEmployeeAsync(Roles.GymAdministrator);

        var command = new PromotePendingGymEmployeeToGymStaffRoleCommand("non-existing-user-id");

        var result = await SendAsync(command);
        result.Type.ShouldBe(ResultTypes.NotFound);
        result.Message.ShouldContain("User not found");
    }

    [Test]
    public async Task ShouldReturnForbiddenIfUserIsNotPendingGymEmployee()
    {
        var user = await CreateUserAsync();

        await RunAsGymEmployeeAsync(Roles.GymAdministrator);

        var command = new PromotePendingGymEmployeeToGymStaffRoleCommand(user.Id);

        var result = await SendAsync(command);
        result.Type.ShouldBe(ResultTypes.Forbidden);
    }

    [Test]
    public async Task ShouldPromotePendingGymEmployee()
    {
        var gymAdminObj = await RunAsGymEmployeeAsync(Roles.GymAdministrator);

        var pendingGymEmployee = await CreateUserAsync(role: Roles.PendingGymEmployee);

        var command = new PromotePendingGymEmployeeToGymStaffRoleCommand(pendingGymEmployee.Id);

        var result = await SendAsync(command);
        result.Succeeded.ShouldBeTrue();

        var rolesAfterPromotion = await GetUserRolesAsync(pendingGymEmployee.Id);

        rolesAfterPromotion.Count.ShouldBe(1);
        rolesAfterPromotion.First().ShouldBe(Roles.GymStaff);

        var gymStaffEmployment = await FindByUserIdAsync<GymEmployment>(pendingGymEmployee.Id);

        gymStaffEmployment.ShouldNotBeNull();
        gymStaffEmployment.GymId.ShouldBe(gymAdminObj.gym.Id);
        gymStaffEmployment.UserId.ShouldBe(pendingGymEmployee.Id);
    }
}
