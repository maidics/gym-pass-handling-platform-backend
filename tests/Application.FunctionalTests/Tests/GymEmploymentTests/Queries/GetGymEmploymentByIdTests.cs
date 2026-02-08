using FitPass.Application.Common.Models;
using FitPass.Application.FunctionalTests.Common.Extensions;

using FitPass.Application.FunctionalTests.TestData;
using FitPass.Application.GymEmployments.Queries;
using FitPass.Domain.Constants;

namespace FitPass.Application.FunctionalTests.Tests.GymEmploymentTests.Queries;

using static Testing;

public class GetGymEmploymentByIdTests : BaseTestFixture
{
    [Test]
    public override void AuthorizeAttributeCheck()
    {
        ShouldRequireAuthorization<GetGymEmploymentByIdQuery>(
            Roles.GymAdministrator,
            Roles.GymStaff
        );
    }

    [Test]
    public async Task ShouldThrowIfParametersAreInvalid()
    {
        await RunAsGymEmployeeAsync(Roles.GymAdministrator);

        var command = new GetGymEmploymentByIdQuery(string.Empty);

        await ShouldThrowIfParametersAreInvalidAsync(command);
    }

    [Test]
    public async Task ShouldReturnNotFoundIfNotFound()
    {
        await RunAsGymEmployeeAsync(Roles.GymAdministrator);

        var command = new GetGymEmploymentByIdQuery("id");

        var result = await SendAsync(command);
        result.ShouldBeFailed(ResultTypes.NotFound);
    }

    [Test]
    public async Task ShouldNotReturnGymEmploymentToAnotherGymForGymEmployee()
    {
        var obj = await TestEntityBuilder.BuildGymEmployeeAsync(Roles.GymAdministrator);

        await RunAsGymEmployeeAsync(Roles.GymAdministrator);

        var command = new GetGymEmploymentByIdQuery(obj.gymEmployment.Id);

        var result = await SendAsync(command);
        result.ShouldBeFailed(ResultTypes.NotFound);
    }

    [Test]
    public async Task ShouldReturnGymEmploymentToGymEmployeeInSameGym()
    {
        var obj = await TestEntityBuilder.BuildGymAsync();

        await RunAsUserAsync(obj.gymAdmin);

        var command = new GetGymEmploymentByIdQuery(obj.gymStaffGymEmployment.Id);

        var result = await SendAsync(command);
        result.ShouldBeSuccessful();

        var gymEmployment = result.Value;
        gymEmployment.ShouldNotBeNull();
        gymEmployment.Id.ShouldBe(obj.gymStaffGymEmployment.Id);
    }
}
