using FitPass.Application.ApplicationUsers.Commands.RoleHandling;
using FitPass.Application.Common.Exceptions;
using FitPass.Application.FunctionalTests.TestData;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;

namespace FitPass.Application.FunctionalTests.Tests.ApplicationUserTests.Commands;

using static Testing;

public class DemoteGymStaffToPendingGymEmployeeTests : BaseTestFixture
{
    [Test]
    public async Task ShouldDenyInvalidUserId() 
    {
        var gymAdmin = await RunAsGymEmployeeAsync(Roles.GymAdministrator);

        var command = new DemoteGymStaffToPendingGymEmployeeCommand(string.Empty);

        var action = () => SendAsync(command);

        action.ShouldThrow<ValidationException>();
    }

    [Test]
    public async Task ShouldDenyDemotionToNonGymStaffUser()
    {
        await RunAsGymEmployeeAsync(Roles.GymAdministrator);

        var user = await ApplicationUserBuilder.BuildAsync();

        var command = new DemoteGymStaffToPendingGymEmployeeCommand(user.Id);

        await Should.ThrowAsync<NotFoundException>(() => SendAsync(command));
    }

    public async Task ShouldDenyDemotionToGymStaffThatIsInAnotherGym()
    {
        var gymStaffGym = await GymBuilder.BuildAsync();

        var gymStaff = await ApplicationUserBuilder
            .WithRole(Roles.GymStaff)
            .BuildAsync();

        var gymStaffEmployment = await GymEmploymentBuilder
            .WithApplicationUserId(gymStaff.Id)
            .WithGymId(gymStaffGym.Id)
            .WithRole(Roles.GymStaff)
            .BuildAsync();

        await RunAsGymEmployeeAsync(Roles.GymAdministrator);

        var command = new DemoteGymStaffToPendingGymEmployeeCommand(gymStaff.Id);

        await Should.ThrowAsync<UnauthorizedAccessException>(() => SendAsync(command));
    }

    [Test]
    public async Task ShouldDemoteGymStaff()
    {
        var obj = await TestEntityBuilder.BuildGymAsync();

        await RunAsUserAsync(obj.gymAdmin);

        var command = new DemoteGymStaffToPendingGymEmployeeCommand(obj.gymStaff.Id);

        await Should.NotThrowAsync(() => SendAsync(command));

        var rolesAfterDemotion = await GetUserRolesAsync(obj.gymStaff.Id);

        rolesAfterDemotion.Count.ShouldBe(1);
        rolesAfterDemotion.First().ShouldBe(Roles.PendingGymEmployee);

        var gymStaffGymEmployment = await FindAsync<GymEmployment>(obj.gymStaffGymEmployment.Id);
        gymStaffGymEmployment.ShouldBeNull();
    }

    [Test]
    public override void AuthorizeAttributeCheck()
    {
        ShouldRequireAuthorization<DemoteGymStaffToPendingGymEmployeeCommand>(Roles.GymAdministrator);
    }
}
