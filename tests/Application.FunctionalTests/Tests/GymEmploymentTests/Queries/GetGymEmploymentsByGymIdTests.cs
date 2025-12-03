using FitPass.Application.Common.Exceptions;
using FitPass.Application.Common.Models;
using FitPass.Application.FunctionalTests.TestData;
using FitPass.Application.GymEmployments.Queries;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;

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
    public async Task ShouldReturnNotFoundIfGymNotFound()
    {
        await RunAsAppAdminAsync();

        var result = await SendAsync(new GetGymEmploymentsByGymIdQuery("non-existing-gym-id"));
        result.Type.ShouldBe(ResultTypes.NotFound);
        result.Message.ShouldContain($"{nameof(Gym)} not found");
    }

    [Test]
    public async Task ShouldReturnGymEmployments()
    {
        var obj = await TestEntityBuilder.BuildGymAsync();

        await RunAsAppAdminAsync();

        var result = await SendAsync(new GetGymEmploymentsByGymIdQuery(obj.gym.Id));
        result.Succeeded.ShouldBeTrue();
        result.Value.ShouldNotBeNull();

        var gymEmployments = result.Value;

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
