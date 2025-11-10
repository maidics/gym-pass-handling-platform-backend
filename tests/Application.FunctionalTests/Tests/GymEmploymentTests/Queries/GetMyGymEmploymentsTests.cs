using FitPass.Application.GymEmployments.Queries;
using FitPass.Domain.Constants;

namespace FitPass.Application.FunctionalTests.Tests.GymEmploymentTests.Queries;

using static Testing;

public class GetMyGymEmploymentsTests : BaseTestFixture
{
    [Test]
    public override void AuthorizeAttributeCheck()
    {
        ShouldRequireAuthorization<GetMyGymEmploymentsQuery>(Roles.GymAdministrator, Roles.GymStaff);
    }

    [Test]
    public async Task ShouldThrowIfGymEmployeeHasNoGymEmployment()
    {
        await RunAsGymEmployeeAsync(Roles.GymAdministrator);

        await Should.ThrowAsync<SystemException>(SendAsync(new GetMyGymEmploymentsQuery()));
    }

    [Test]
    public async Task ShouldReturnGymEmployments()
    {
        var gymAdminObj = await RunAsGymEmployeeAsync(Roles.GymAdministrator);

        var gymStaff = await ApplicationUserBuilder
            .WithRole(Roles.GymStaff)
            .BuildAsync();

        var gymStaffGymEmployment = await GymEmploymentBuilder
            .WithApplicationUserId(gymStaff.Id)
            .WithGymId(gymAdminObj.gym.Id)
            .WithRole(Roles.GymStaff)
            .BuildAsync();

        var gymStaffProfile = await UserProfileBuilder
            .WithFirstName("GymStaffFirst")
            .WithLastName("GymStaffLast")
            .WithApplicationUserId(gymStaff.Id)
            .BuildAsync();

        var gymEmployments = await SendAsync(new GetMyGymEmploymentsQuery());

        gymEmployments.Count.ShouldBe(2);
        var gymAdminGE = gymEmployments.FirstOrDefault(ge => ge.ApplicationUserId == gymAdminObj.user.Id);
        gymAdminGE.ShouldNotBeNull();
        gymAdminGE.GymId.ShouldBe(gymAdminObj.gym.Id);
        gymAdminGE.ApplicationUserId.ShouldBe(gymAdminObj.user.Id);
        gymAdminGE.Role.ShouldBe(Roles.GymAdministrator);

        var gymStaffGE = gymEmployments.FirstOrDefault(ge => ge.ApplicationUserId == gymStaff.Id);
        gymStaffGE.ShouldNotBeNull();
        gymStaffGE.GymId.ShouldBe(gymAdminObj.gym.Id);
        gymStaffGE.ApplicationUserId.ShouldBe(gymStaff.Id);
        gymStaffGE.Role.ShouldBe(Roles.GymStaff);

        var userProfiles = gymEmployments.Select(ge => ge.UserProfile);
        userProfiles.ShouldNotBeNull();
        userProfiles.Count().ShouldBe(2);
        var gymAdminUpWithEmail = userProfiles.FirstOrDefault(up => up.ApplicationUserId == gymAdminObj.user.Id);
        gymAdminUpWithEmail.ShouldNotBeNull();
        gymAdminUpWithEmail.Email.ShouldNotBeNull();
        gymAdminUpWithEmail.FirstName.ShouldBe("GymAdminFirst");
        gymAdminUpWithEmail.LastName.ShouldBe("GymAdminLast");

        var gymStaffUpWithEmail = userProfiles.FirstOrDefault(up => up.ApplicationUserId == gymStaff.Id);
        gymStaffUpWithEmail.ShouldNotBeNull();
        gymStaffUpWithEmail.Email.ShouldNotBeNull();
        gymStaffUpWithEmail.FirstName.ShouldBe("GymStaffFirst");
        gymStaffUpWithEmail.LastName.ShouldBe("GymStaffLast");
    }
}
