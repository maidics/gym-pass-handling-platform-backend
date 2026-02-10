using FitPass.Application.Common.Models;
using FitPass.Application.FunctionalTests.Common.Extensions;
using FitPass.Application.FunctionalTests.TestData;
using FitPass.Application.GymPassProducts.Queries;
using FitPass.Domain.Constants;
using FitPass.Domain.Enums;
using FitPass.Domain.ValueObjects;

namespace FitPass.Application.FunctionalTests.Tests.GymPassProductTests.Queries;

using static Testing;

public class GetGymPassProductByIdTests : BaseTestFixture
{
    [Test]
    public override void AuthorizeAttributeCheck()
    {
        ShouldNotRequireAuthorization<GetGymPassProductByIdQuery>();
    }

    [Test]
    public async Task ShouldThrowIfParametersAreInvalid()
    {
        var query = new GetGymPassProductByIdQuery(string.Empty);

        await ShouldThrowIfParametersAreInvalidAsync(query);
    }

    [Test]
    public async Task ShouldReturnNotFoundIfGymPassProductIsNotFound()
    {
        var query = new GetGymPassProductByIdQuery("id");

        var result = await SendAsync(query);
        result.ShouldBeFailed(ResultTypes.NotFound);
    }

    [Test]
    public async Task ShouldReturnGymPassProduct()
    {
        var gymAdmin = await CreateUserAsync(role: Roles.GymAdministrator);

        var gymPassProduct = await TestEntityBuilder.BuildGymPassProductWithPaymentProfile(
            gymAdmin,
            new Money(10, CurrencyCode.EUR)
        );

        var query = new GetGymPassProductByIdQuery(gymPassProduct.Id);

        var result = await SendAsync(query);
        result.ShouldBeSuccessful();

        var dto = result.Value;
        dto.Id.ShouldBe(gymPassProduct.Id);
    }
}
