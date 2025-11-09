using FitPass.Application.ApplicationUsers.Commands.RoleHandling;
using FitPass.Application.Common.Exceptions;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;

namespace FitPass.Application.FunctionalTests.Tests.ApplicationUserTests.Commands;

using static Testing;

public class DemoteGymStaffToPendingGymEmployeeTests : BaseTestFixture
{
    [Test]
    public async Task ShouldDenyInvalidUserId() 
    {
        var gymAdmin = await RunAsGymAdminAsync();

        var command = new DemoteGymStaffToPendingGymEmployeeCommand(string.Empty);

        var action = () => SendAsync(command);

        action.ShouldThrow<ValidationException>();
    }

    [Test]
    public async Task ShouldDenyDemotionToNonGymStaffUser()
    {
        var gym = await GymBuilder.BuildAsync();

        var gymAdmin = await ApplicationUserBuilder
            .WithRole(Roles.GymAdministrator)
            .BuildAsync();

        var gymAdminEmployment = await GymEmploymentBuilder
            .WithApplicationUserId(gymAdmin.Id)
            .WithGymId(gym.Id)
            .WithRole(Roles.GymAdministrator)
            .BuildAsync();

        await RunAsUserAsync(gymAdmin);

        var user = await ApplicationUserBuilder.BuildAsync();

        var command = new DemoteGymStaffToPendingGymEmployeeCommand(user.Id);

        await Should.ThrowAsync<NotFoundException>(() => SendAsync(command));
    }

    public async Task ShouldDenyDemotionToGymStaffThatIsInAnotherGym()
    {
        var gymAdminGym = await GymBuilder.BuildAsync();

        var gymAdmin = await ApplicationUserBuilder
            .WithRole(Roles.GymAdministrator)
            .BuildAsync();

        var gymAdminEmployment = await GymEmploymentBuilder
            .WithApplicationUserId(gymAdmin.Id)
            .WithGymId(gymAdminGym.Id)
            .WithRole(Roles.GymAdministrator)
            .BuildAsync();

        var gymStaffGym = await GymBuilder.BuildAsync();

        var gymStaff = await ApplicationUserBuilder
            .WithRole(Roles.GymStaff)
            .BuildAsync();

        var gymStaffEmployment = await GymEmploymentBuilder
            .WithApplicationUserId(gymStaff.Id)
            .WithGymId(gymStaffGym.Id)
            .WithRole(Roles.GymStaff)
            .BuildAsync();

        await RunAsUserAsync(gymAdmin);

        var command = new DemoteGymStaffToPendingGymEmployeeCommand(gymStaff.Id);

        await Should.ThrowAsync<UnauthorizedAccessException>(() => SendAsync(command));
    }

    [Test]
    public async Task ShouldDemoteGymStaff()
    {
        var gym = await GymBuilder.BuildAsync();

        var gymAdmin = await ApplicationUserBuilder
            .WithRole(Roles.GymAdministrator)
            .BuildAsync();

        var gymAdminEmployment = await GymEmploymentBuilder
            .WithApplicationUserId(gymAdmin.Id)
            .WithGymId(gym.Id)
            .WithRole(Roles.GymAdministrator)
            .BuildAsync();

        var gymStaff = await ApplicationUserBuilder
            .WithRole(Roles.GymStaff)
            .BuildAsync();

        var gymStaffEmployment = await GymEmploymentBuilder
            .WithApplicationUserId(gymStaff.Id)
            .WithGymId(gym.Id)
            .WithRole(Roles.GymStaff)
            .BuildAsync();

        await RunAsUserAsync(gymAdmin);

        var command = new DemoteGymStaffToPendingGymEmployeeCommand(gymStaff.Id);

        await Should.NotThrowAsync(() => SendAsync(command));

        var rolesAfterDemotion = await GetUserRolesAsync(gymStaff.Id);

        rolesAfterDemotion.Count.ShouldBe(1);
        rolesAfterDemotion.First().ShouldBe(Roles.PendingGymEmployee);

        var gymStaffGymEmployment = await FindAsync<GymEmployment>(gymStaffEmployment.Id);

        gymStaffGymEmployment.ShouldBeNull();
    }

    [Test]
    public override void AuthorizeAttributeCheck()
    {
        var hasAuthorizationAttribute = HasAuthorizeAttribute<DemoteGymStaffToPendingGymEmployeeCommand>();

        hasAuthorizationAttribute.ShouldBeTrue();
    }
}
