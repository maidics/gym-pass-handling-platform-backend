using FitPass.Application.Common.Models;
using FitPass.Application.FunctionalTests.TestData;
using FitPass.Application.GymPassProducts.Queries;
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
    public async Task ShouldDenyInvalidParameters()
    {
        var query = new GetGymPassProductsByGymIdQuery(string.Empty);

        await ShouldThrowIfParametersAreInvalidAsync(query);
    }

    [Test]
    public async Task ShouldReturnNotFoundIfGymIsNotFound()
    {
        var query = new GetGymPassProductsByGymIdQuery("gymId");

        var result = await SendAsync(query);
        result.Type.ShouldBe(ResultTypes.NotFound);
        result.Message.ShouldNotBeEmpty();
    }
    
    [Test]
    public async Task ShouldReturnGymPassProductsIfGymIsFound()
    {
        var obj = await TestEntityBuilder.BuildGymWithTenantPaymentProfileAsync();
        
        var product1 = await TestEntityBuilder.BuildGymPassProduct(
            obj.gymAdmin, 
            Money.Usd(10), 
            type: PassType.MultiUse, 
            totalUses: 5, 
            daysAfterExpiring: null);
        
        var product2 = await TestEntityBuilder.BuildGymPassProduct(
            obj.gymAdmin, 
            Money.Usd(10),
            type: PassType.Unlimited,
            totalUses: null,
            daysAfterExpiring: 10);

        var query = new GetGymPassProductsByGymIdQuery(obj.gym.Id);

        var result = await SendAsync(query);
        result.Type.ShouldBe(ResultTypes.Success);
        result.Value.ShouldNotBeNull();

        var dtos = result.Value;
        
        dtos.Count.ShouldBe(3);
        
        var product1Dto = dtos.FirstOrDefault(x => x.Id == product1.Id);
        product1Dto.ShouldNotBeNull();
        product1Dto.AssertTo(product1);
        
        var product2Dto = dtos.FirstOrDefault(x => x.Id == product2.Id);
        product2Dto.ShouldNotBeNull();
        product2Dto.AssertTo(product2);

        var product3Dto = dtos.FirstOrDefault(x => x.Id == obj.gymPassProduct.Id);
        product3Dto.ShouldNotBeNull();
        product3Dto.AssertTo(obj.gymPassProduct);
    }
}
