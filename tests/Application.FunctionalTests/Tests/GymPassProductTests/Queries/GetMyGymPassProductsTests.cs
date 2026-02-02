using FitPass.Application.FunctionalTests.Infrastructure.Testing;
using FitPass.Application.FunctionalTests.TestData;
using FitPass.Application.GymPassProducts.Queries;
using FitPass.Domain.Constants;

namespace FitPass.Application.FunctionalTests.Tests.GymPassProductTests.Queries;

using static Testing;

public class GetMyGymPassProductsTests : BaseTestFixture
{
    [Test]
    public override void AuthorizeAttributeCheck()
    {
        ShouldRequireAuthorization<GetMyGymPassProductsQuery>(
            Roles.GymAdministrator,
            Roles.GymStaff
        );
    }

    [Test]
    public async Task ShouldReturnGymPassProducts()
    {
        var obj = await TestEntityBuilder.BuildGymAsync();

        await RunAsUserAsync(obj.gymAdmin);

        var result = await SendAsync(new GetMyGymPassProductsQuery());

        result.Count.ShouldBe(1);
        result.Count(x => x.Id == obj.gymPassProduct.Id).ShouldBe(1);
    }
}
