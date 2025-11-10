using FitPass.Application.GymEmployments.Queries;
using FitPass.Domain.Constants;

namespace FitPass.Application.FunctionalTests.Tests.GymEmploymentTests.Commands;

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
        await RunAsGymAdminAsync();

        await Should.ThrowAsync<SystemException>(SendAsync(new GetMyGymEmploymentsQuery()));
    }

    [Test]
    public async Task ShouldReturnGymEmployments()
    {
        var gym = await GymBuilder.BuildAsync();

        var gymAdmin = await RunAsGymAdminAsync();

        var gymAdminGymEmployment = await GymEmploymentBuilder
            .WithApplicationUserId(gymAdmin.Id)
            .WithGymId(gym.Id)
            .WithRole(Roles.GymAdministrator)
            .BuildAsync();

        var gymAdminProfile = await UserProfileBuilder
            .WithFirstName("GymAdminFirst")
            .WithLastName("GymAdminLast")
            .WithApplicationUserId(gymAdmin.Id)
            .BuildAsync();

        var gymStaff = await ApplicationUserBuilder
            .WithRole(Roles.GymStaff)
            .BuildAsync();

        var gymStaffGymEmployment = await GymEmploymentBuilder
            .WithApplicationUserId(gymStaff.Id)
            .WithGymId(gym.Id)
            .WithRole(Roles.GymStaff)
            .BuildAsync();

        var gymStaffProfile = await UserProfileBuilder
            .WithFirstName("GymStaffFirst")
            .WithLastName("GymStaffLast")
            .WithApplicationUserId(gymStaff.Id)
            .BuildAsync();

        var gymEmployments = await SendAsync(new GetMyGymEmploymentsQuery());

        gymEmployments.Count.ShouldBe(2);
        var gymAdminGE = gymEmployments.FirstOrDefault(ge => ge.ApplicationUserId == gymAdmin.Id);
        gymAdminGE.ShouldNotBeNull();
        gymAdminGE.GymId.ShouldBe(gym.Id);
        gymAdminGE.ApplicationUserId.ShouldBe(gymAdmin.Id);
        gymAdminGE.Role.ShouldBe(Roles.GymAdministrator);

        var gymStaffGE = gymEmployments.FirstOrDefault(ge => ge.ApplicationUserId == gymStaff.Id);
        gymStaffGE.ShouldNotBeNull();
        gymStaffGE.GymId.ShouldBe(gym.Id);
        gymStaffGE.ApplicationUserId.ShouldBe(gymStaff.Id);
        gymStaffGE.Role.ShouldBe(Roles.GymStaff);

        var userProfiles = gymEmployments.Select(ge => ge.UserProfile);
        userProfiles.ShouldNotBeNull();
        userProfiles.Count().ShouldBe(2);
        var gymAdminUpWithEmail = userProfiles.FirstOrDefault(up => up.ApplicationUserId == gymAdmin.Id);
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
