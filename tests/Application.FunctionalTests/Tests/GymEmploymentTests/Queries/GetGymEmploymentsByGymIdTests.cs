using FitPass.Application.Common.Models;
using FitPass.Application.FunctionalTests.Infrastructure.Testing;
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
    public async Task ShouldThrowIfParametersAreInvalid()
    {
        await RunAsAppAdminAsync();

        var command = new GetGymEmploymentByIdQuery(string.Empty);

        await ShouldThrowIfParametersAreInvalidAsync(command);
    }

    [Test]
    public async Task ShouldReturnNotFoundIfGymNotFound()
    {
        await RunAsAppAdminAsync();

        var result = await SendAsync(new GetGymEmploymentsByGymIdQuery("id"));
        result.ShouldBeFailed(ResultTypes.NotFound);
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
        gymEmployments
            .Count(x =>
                x.Id == obj.gymAdminGymEmployment.Id || x.Id == obj.gymStaffGymEmployment.Id
            )
            .ShouldBe(2);
    }
}
