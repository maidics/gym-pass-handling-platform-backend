using FitPass.Application.GymEmployments.Queries;
using FitPass.Domain.Constants;

namespace FitPass.Application.FunctionalTests.Tests.GymEmploymentTests.Queries;

using static Testing;

public class GetMyGymEmploymentTests : BaseTestFixture
{
    [Test]
    public override void AuthorizeAttributeCheck()
    {
        ShouldRequireAuthorization<GetMyGymEmploymentQuery>(Roles.GymAdministrator, Roles.GymStaff);
    }

    [Test]
    public async Task ShouldThrowIfGymEmployeeHasNoGymEmployment()
    {
        var gymStaff = await ApplicationUserBuilder
            .WithRole(Roles.GymStaff)
            .BuildAsync();

        await RunAsUserAsync(gymStaff);

        var command = new GetMyGymEmploymentQuery();

        await Should.ThrowAsync<SystemException>(SendAsync(command));
    }

    [Test]
    public async Task ShouldReturnGymEmployment()
    {
        var gymAdminObj = await RunAsGymEmployeeAsync(Roles.GymAdministrator);

        var ge = await SendAsync(new GetMyGymEmploymentQuery());
        ge.ShouldNotBeNull();
        ge.ApplicationUserId.ShouldBe(gymAdminObj.user.Id);
        ge.GymId.ShouldBe(gymAdminObj.gym.Id);
        ge.Role.ShouldBe(gymAdminObj.gymEmployment.Role);

        ge.UserProfile.Email.ShouldBe(gymAdminObj.user.Email);
        ge.UserProfile.FirstName.ShouldBe(gymAdminObj.userProfile.FirstName);
        ge.UserProfile.LastName.ShouldBe(gymAdminObj.userProfile.LastName);
        ge.UserProfile.ApplicationUserId.ShouldBe(gymAdminObj.user.Id);
    }
}
