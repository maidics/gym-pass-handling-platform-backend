using FitPass.Application.FunctionalTests.TestData;
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
        var gymAdmin = await ApplicationUserBuilder
            .WithRole(Roles.GymAdministrator)
            .BuildAsync();

        await RunAsUserAsync(gymAdmin);

        await Should.ThrowAsync<SystemException>(SendAsync(new GetMyGymEmploymentsQuery()));
    }

    [Test]
    public async Task ShouldReturnGymEmployments()
    {
        var obj = await TestEntityBuilder.BuildGymAsync();

        await RunAsUserAsync(obj.gymAdmin);

        var gymEmployments = await SendAsync(new GetMyGymEmploymentsQuery());
        gymEmployments.Count.ShouldBe(2);

        gymEmployments
            .FirstOrDefault(ge => ge.ApplicationUserId == obj.gymAdmin.Id)
            .AssertTo(obj.gymAdminGymEmployment, obj.gymAdminUserProfile, obj.gymAdmin);

        gymEmployments
            .FirstOrDefault(ge => ge.ApplicationUserId == obj.gymStaff.Id)
            .AssertTo(obj.gymStaffGymEmployment, obj.gymStaffUserProfile, obj.gymStaff);
    }
}
