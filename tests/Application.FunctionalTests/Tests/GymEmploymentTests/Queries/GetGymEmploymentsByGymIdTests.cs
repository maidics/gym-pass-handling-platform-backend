using System.ComponentModel.DataAnnotations;
using FitPass.Application.FunctionalTests.TestData;
using FitPass.Application.GymEmployments.Queries;
using FitPass.Domain.Constants;

namespace FitPass.Application.FunctionalTests.Tests.GymEmploymentTests.Queries;

using static Testing;

public class GetGymEmploymentsByGymIdTests : BaseTestFixture
{
    [Test]
    public override void AuthorizeAttributeCheck()
    {
        ShouldRequireAuthorization<GetGymEmploymentsByGymIdQuery>(Roles.AppAdministrator);
    }

    [Test]
    public async Task ShouldDenyInvalidParameter()
    {
        await RunAsAppAdminAsync();

        await Should.ThrowAsync<ValidationException>(SendAsync(new GetGymEmploymentsByGymIdQuery(string.Empty)));
    }

    [Test]
    public async Task ShouldThrowIfGymDoesNotExist()
    {
        await RunAsAppAdminAsync();

        await Should.ThrowAsync<NotFoundException>(SendAsync(new GetGymEmploymentsByGymIdQuery("invalidGymId")));
    }

    [Test]
    public async Task ShouldReturnGymEmployments()
    {
        var obj = await TestEntityBuilder.BuildGymAsync();

        await RunAsAppAdminAsync();

        var gymEmployments = await SendAsync(new GetGymEmploymentsByGymIdQuery(obj.gym.Id));
        gymEmployments.ShouldNotBeNull();
        gymEmployments.Count.ShouldBe(2);

        gymEmployments
            .FirstOrDefault(ge => ge.ApplicationUserId == obj.gymAdmin.Id)
            .AssertTo(obj.gymAdminGymEmployment, obj.gymAdminUserProfile, obj.gymAdmin);


        gymEmployments
            .FirstOrDefault(ge => ge.ApplicationUserId == obj.gymStaff.Id)
            .AssertTo(obj.gymStaffGymEmployment, obj.gymStaffUserProfile, obj.gymStaff);
    }
}
