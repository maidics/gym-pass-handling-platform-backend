using FitPass.Application.FunctionalTests.TestData;
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
    public async Task ShouldReturnGymEmployment()
    {
        var obj = await RunAsGymEmployeeAsync(Roles.GymAdministrator);

        var gymEmployment = await SendAsync(new GetMyGymEmploymentQuery());
        gymEmployment.ShouldNotBeNull();
        gymEmployment.AssertTo(obj.gymEmployment, obj.userProfile, obj.user);
    }
}
