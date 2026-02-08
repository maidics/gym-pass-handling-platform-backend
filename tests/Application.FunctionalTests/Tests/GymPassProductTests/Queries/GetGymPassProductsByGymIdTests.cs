using FitPass.Application.Common.Models;
using FitPass.Application.FunctionalTests.Common.Extensions;

using FitPass.Application.FunctionalTests.TestData;
using FitPass.Application.GymPassProducts.Queries;
using FitPass.Domain.Entities;
using FitPass.Domain.Enums;
using FitPass.Domain.ValueObjects;

namespace FitPass.Application.FunctionalTests.Tests.GymPassProductTests.Queries;

using static Testing;

public class GetGymPassProductsByGymIdTests : BaseTestFixture
{
    [Test]
    public override void AuthorizeAttributeCheck()
    {
        ShouldNotRequireAuthorization<GetGymPassProductsByGymIdQuery>();
    }

    [Test]
    public async Task ShouldThrowIfParametersAreInvalid()
    {
        var query = new GetGymPassProductsByGymIdQuery(string.Empty);

        await ShouldThrowIfParametersAreInvalidAsync(query);
    }

    [Test]
    public async Task ShouldReturnNotFoundIfGymIsNotFound()
    {
        var query = new GetGymPassProductsByGymIdQuery("id");

        var result = await SendAsync(query);
        result.ShouldBeFailed(ResultTypes.NotFound);
    }

    [Test]
    public async Task ShouldReturnGymPassProducts()
    {
        var obj = await TestEntityBuilder.BuildGymWithTenantPaymentProfileAsync();

        var product = GymPassProduct.SingleUse(
            obj.gym.Id,
            "Product Name",
            "Product Description",
            true,
            new Money(10, CurrencyCode.EUR)
        );

        await AddAsync(product);

        var query = new GetGymPassProductsByGymIdQuery(obj.gym.Id);

        var result = await SendAsync(query);
        result.ShouldBeSuccessful();

        var dtos = result.Value;
        dtos.Count.ShouldBe(2);
        dtos.Count(x => x.Id == product.Id || x.Id == obj.gymPassProduct.Id).ShouldBe(2);
    }
}
