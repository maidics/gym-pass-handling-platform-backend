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
        var obj = await RunAsGymEmployeeAsync(Roles.GymAdministrator);

        var ge = await SendAsync(new GetMyGymEmploymentQuery());
        ge.ShouldNotBeNull();
        ge.AssertTo(obj.gymEmployment, obj.userProfile, obj.user);
    }
}
