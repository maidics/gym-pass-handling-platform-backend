using FitPass.Application.Common.Models;
using FitPass.Application.FunctionalTests.TestData;
using FitPass.Application.Gyms.Queries;
using FitPass.Domain.Constants;

namespace FitPass.Application.FunctionalTests.Tests.GymTests.Queries;

using static Testing;

public class GetGymByIdTests : BaseTestFixture
{
    [Test]
    public override void AuthorizeAttributeCheck()
    {
        ShouldRequireAuthorization<GetGymByIdQuery>(Roles.AppAdministrator);
    }

    [Test]
    public async Task ShouldDenyInvalidParameters()
    {
        await RunAsAppAdminAsync();

        var query = new GetGymByIdQuery(string.Empty);

        await ShouldThrowIfParametersAreInvalid(query);
    }

    [Test]
    public async Task ShouldReturnNotFoundIfGymNotExists()
    {
        await RunAsAppAdminAsync();

        var query = new GetGymByIdQuery("gymId");

        var result = await SendAsync(query);
        result.Type.ShouldBe(ResultTypes.NotFound);
        result.Message.ShouldContain("Gym not found");
    }

    [Test]
    public async Task ShouldReturnGym()
    {
        var obj = await TestEntityBuilder.BuildGymAsync();

        await RunAsAppAdminAsync();

        var query = new GetGymByIdQuery(obj.gym.Id);

        var result = await SendAsync(query);
        result.Succeeded.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.AssertToGym(obj.gym);
    }
}
