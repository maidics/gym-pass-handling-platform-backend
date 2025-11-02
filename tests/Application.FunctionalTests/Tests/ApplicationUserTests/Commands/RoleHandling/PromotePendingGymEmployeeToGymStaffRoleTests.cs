
using FitPass.Application.ApplicationUsers.Commands.RoleHandling;
using FitPass.Application.Common.Exceptions;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;

namespace FitPass.Application.FunctionalTests.Tests.ApplicationUserTests.Commands.RoleHandling;

using static Testing;

public class PromotePendingGymEmployeeToGymStaffRoleTests : BaseTestFixture
{

    [Test]
    public override void AuthorizeAttributeCheck()
    {
        var command = new PromotePendingGymEmployeeToGymStaffRoleCommand("UserId");

        var hasAttribute = HasAuthorizeAttribute<PromotePendingGymEmployeeToGymStaffRoleCommand>();

        hasAttribute.ShouldBeTrue();
    }

    [Test]
    public async Task ShouldDenyInvalidUserId()
    {
        await RunAsGymAdminAsync();

        var command = new PromotePendingGymEmployeeToGymStaffRoleCommand(string.Empty);

        await Should.ThrowAsync<ValidationException>(() => SendAsync(command));
    }

    [Test]
    public async Task ShouldPromotePendingGymEmployee()
    {
        var gym = await GymBuilder.BuildAsync();

        var gymAdmin = await ApplicationUserBuilder
            .WithRole(Roles.GymAdministrator)
            .BuildAsync();

        var gymAdminEmployment = await GymEmploymentBuilder
            .WithRole(Roles.GymAdministrator)
            .WithApplicationUserId(gymAdmin.Id)
            .WithGymId(gym.Id)
            .BuildAsync();

        var pendingGymEmployee = await ApplicationUserBuilder
            .WithRole(Roles.PendingGymEmployee)
            .BuildAsync();

        await RunAsUserAsync(gymAdmin);

        var command = new PromotePendingGymEmployeeToGymStaffRoleCommand(pendingGymEmployee.Id);

        await Should.NotThrowAsync(() => SendAsync(command));

        var rolesAfterPromotion = await GetRoles(pendingGymEmployee.Id);

        rolesAfterPromotion.Count.ShouldBe(1);
        rolesAfterPromotion.First().ShouldBe(Roles.GymStaff);

        var gymStaffEmployment = await FindByApplicationUserIdAsync<GymEmployment>(pendingGymEmployee.Id);

        gymStaffEmployment.ShouldNotBeNull();
        gymStaffEmployment.GymId.ShouldBe(gym.Id);
        gymStaffEmployment.ApplicationUserId.ShouldBe(pendingGymEmployee.Id);
    }

    [Test]
    public async Task ShouldNotPromoteNonPendingGymEmployee()
    {
        var gym = await GymBuilder.BuildAsync();

        var gymAdmin = await ApplicationUserBuilder
            .WithRole(Roles.GymAdministrator)
            .BuildAsync();

        var gymAdminEmployment = await GymEmploymentBuilder
            .WithRole(Roles.GymAdministrator)
            .WithApplicationUserId(gymAdmin.Id)
            .WithGymId(gym.Id)
            .BuildAsync();

        var user = await ApplicationUserBuilder
            .BuildAsync();

        var command = new PromotePendingGymEmployeeToGymStaffRoleCommand(user.Id);

        await RunAsUserAsync(gymAdmin);

        await Should.ThrowAsync<ForbiddenAccessException>(() => SendAsync(command));
    }

    [Test]
    public async Task ShouldThrowIfUserWithGivenUserIdDoesNotExist()
    {
        var gym = await GymBuilder.BuildAsync();

        var gymAdmin = await ApplicationUserBuilder
            .WithRole(Roles.GymAdministrator)
            .BuildAsync();

        var gymAdminEmployment = await GymEmploymentBuilder
            .WithRole(Roles.GymAdministrator)
            .WithApplicationUserId(gymAdmin.Id)
            .WithGymId(gym.Id)
            .BuildAsync();

        var command = new PromotePendingGymEmployeeToGymStaffRoleCommand("does-not-exist");

        await RunAsUserAsync(gymAdmin);

        await Should.ThrowAsync<NotFoundException>(() => SendAsync(command));
    }
}
