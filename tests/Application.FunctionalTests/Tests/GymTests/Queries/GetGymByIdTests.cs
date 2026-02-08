using FitPass.Application.Common.Models;
using FitPass.Application.FunctionalTests.Common.Extensions;

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
        ShouldNotRequireAuthorization<GetGymByIdQuery>();
    }

    [Test]
    public async Task ShouldThrowIfParametersAreInvalid()
    {
        await RunAsAppAdminAsync();

        var query = new GetGymByIdQuery(string.Empty);

        await ShouldThrowIfParametersAreInvalidAsync(query);
    }

    [Test]
    public async Task ShouldReturnNotFoundIfGymNotFound()
    {
        await RunAsAppAdminAsync();

        var query = new GetGymByIdQuery("id");

        var result = await SendAsync(query);
        result.ShouldBeFailed(ResultTypes.NotFound);
    }

    [Test]
    public async Task ShouldReturnGym()
    {
        var obj = await TestEntityBuilder.BuildGymWithTenantPaymentProfileAsync();

        var query = new GetGymByIdQuery(obj.gym.Id);

        var result = await SendAsync(query);
        result.ShouldBeSuccessful();

        var gymDto = result.Value;
        gymDto.ShouldNotBeNull();
        gymDto.Id.ShouldBe(obj.gym.Id);
        gymDto.PaymentProfile.ShouldBeNull();
    }

    [Test]
    public async Task ShouldReturnGymWithPaymentProfileForGymAdminIfTheirGym()
    {
        var obj = await TestEntityBuilder.BuildGymWithTenantPaymentProfileAsync();

        await RunAsUserAsync(obj.gymAdmin);

        var query = new GetGymByIdQuery(obj.gym.Id);

        var result = await SendAsync(query);
        result.ShouldBeSuccessful();

        var gymDto = result.Value;
        gymDto.Id.ShouldBe(obj.gym.Id);
        gymDto.PaymentProfile.ShouldNotBeNull();
    }

    [Test]
    public async Task ShouldReturnGymWithoutPaymentProfileForGymAdminFromAnotherGym()
    {
        var obj = await TestEntityBuilder.BuildGymWithTenantPaymentProfileAsync();

        await RunAsGymEmployeeAsync(Roles.GymAdministrator);

        var query = new GetGymByIdQuery(obj.gym.Id);

        var result = await SendAsync(query);
        result.ShouldBeSuccessful();

        var gymDto = result.Value;
        gymDto.Id.ShouldBe(obj.gym.Id);
        gymDto.PaymentProfile.ShouldBeNull();
    }
}
