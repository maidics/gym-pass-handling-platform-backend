using FitPass.Application.Common.Models;
using FitPass.Application.FunctionalTests.TestData;
using FitPass.Application.GymPassProducts.Commands;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using FitPass.Domain.Entities.Payment;
using FitPass.Domain.Enums;
using FitPass.Domain.ValueObjects;

namespace FitPass.Application.FunctionalTests.Tests.GymPassProductTests.Commands;

using static Testing;

public class UpdateGymPassProductTests : BaseTestFixture
{
    [Test]
    public override void AuthorizeAttributeCheck()
    {
        ShouldRequireAuthorization<UpdateGymPassProductCommand>(Roles.GymAdministrator);
    }

    [TestCase(null, null)]
    [TestCase(1, 2)]
    public async Task ShouldDenyInvalidParameters(int? totalUses, int? daysAfterExpiring)
    {
        await RunAsGymEmployeeAsync(Roles.GymAdministrator);

        var command = new UpdateGymPassProductCommand(
            string.Empty,
            string.Empty,
            string.Empty,
            totalUses,
            daysAfterExpiring,
            Money.Zero("eur"));

        await ShouldThrowIfParametersAreInvalidAsync(command);
    }

    [Test]
    public async Task ShouldReturnBusinessRuleViolationIfTheGymIsSuspended()
    {
        var obj = await TestEntityBuilder.BuildGymAsync(GymStatus.Suspended);

        await RunAsUserAsync(obj.gymAdmin);
        
        var command = new UpdateGymPassProductCommand(
            "gymPassProductId", 
            "Updated Name", 
            "Updated Description", 
            1, 
            null, 
            Money.Zero("eur"));
        
        var result =  await SendAsync(command);
        result.Type.ShouldBe(ResultTypes.BusinessRuleViolation);
        result.Message.ShouldNotBeEmpty();
    }

    [Test]
    public async Task ShouldReturnNotFoundIfGymPassProductNotFound()
    {
        var obj = await TestEntityBuilder.BuildGymWithTenantPaymentProfileAsync();
        var product = await TestEntityBuilder.BuildGymPassProduct(obj.gymAdmin, Money.Zero("usd"));
        
        await RunAsUserAsync(obj.gymAdmin);
        
        var command = new UpdateGymPassProductCommand(
            product.Id, 
            "Updated Name", 
            "Updated Description", 
            1, 
            null, 
            Money.Zero("eur"));
        
        var result =  await SendAsync(command);
        result.Type.ShouldBe(ResultTypes.NotFound);
        result.Message.ShouldNotBeEmpty();
    }
    
    [TestCase(PassType.SingleUse, 1, null, "Updated Name", "Updated Description", 5, null, 49.99, "eur")]
    [TestCase(PassType.MultiUse, 10, null, "Updated Name", "Updated Description", 20, null, 79.99, "gbp")]
    [TestCase(PassType.Unlimited, null, 30, "Updated Name", "Updated Description", null, 60, 99.99, "usd")]
    public async Task ShouldUpdateGymPassProduct(
        PassType passType, int? passTotalUses, int? passDaysAfterExpiring,
        string newName, string newDescription, int? newTotalUses, int? newDaysAfterExpiring, decimal newMoneyAmount, string newMoneyCurrency)
    {
        var obj = await TestEntityBuilder.BuildGymWithTenantPaymentProfileAsync();
        var product = await TestEntityBuilder.BuildGymPassProduct(obj.gymAdmin, Money.Zero("usd"));
        
        var paymentIdentity = await GetFirstAsync<ProductPaymentIdentity>();
        paymentIdentity.ShouldNotBeNull();
        paymentIdentity.PriceId.ShouldNotBeNullOrEmpty();
        paymentIdentity.ProductId.ShouldNotBeNullOrEmpty();
        
        string oldProductId = paymentIdentity.ProductId;
        string oldPriceId = paymentIdentity.PriceId;
        
        await RunAsUserAsync(obj.gymAdmin);
        
        var command = new UpdateGymPassProductCommand(
            product.Id, 
            newName, 
            newDescription, 
            newTotalUses, 
            newDaysAfterExpiring, 
            new Money(newMoneyAmount, newMoneyCurrency));
        
        var result =  await SendAsync(command);
        result.Succeeded.ShouldBeTrue();
        result.Value.ShouldNotBeNull();

        var productDto = result.Value;
        productDto.Id.ShouldBe(product.Id);
        productDto.Name.ShouldBe(newName);
        productDto.Description.ShouldBe(newDescription);
        productDto.TotalUses.ShouldBe(newTotalUses);
        productDto.DaysAfterExpiring.ShouldBe(newDaysAfterExpiring);
        productDto.Price.Amount.ShouldBe(newMoneyAmount);
        productDto.Price.Currency.ShouldBe(newMoneyCurrency);
        
        product = await FindAsync<GymPassProduct>(product.Id);
        product.ShouldNotBeNull();
        product.Name.ShouldBe(newName);
        product.Description.ShouldBe(newDescription);
        product.TotalUses.ShouldBe(newTotalUses);
        product.DaysAfterExpires.ShouldBe(newDaysAfterExpiring);
        product.Price.Amount.ShouldBe(newMoneyAmount);
        product.Price.Currency.ShouldBe(newMoneyCurrency);

        paymentIdentity = await GetFirstAsync<ProductPaymentIdentity>();
        paymentIdentity.ShouldNotBeNull();
        paymentIdentity.PriceId.ShouldNotBe(oldPriceId);
        paymentIdentity.ProductId.ShouldBe(oldProductId);
    }

    [TestCase(100, "usd", "Test Product", "Test Description", PassType.SingleUse, 1, null)]
    public async Task ShouldNotUpdateGymPassProductIfParametersAreEqual(
        decimal moneyAmount, string moneyCurrency, string name, string description, PassType passType, int? totalUses, int? daysAfterExpiring)
    {
        var obj = await TestEntityBuilder.BuildGymAsync();
        var product = await TestEntityBuilder.BuildGymPassProduct(
            obj.gymAdmin,
            new Money(moneyAmount, moneyCurrency),
            name,
            description,
            passType,
            totalUses,
            daysAfterExpiring);

        await RunAsUserAsync(obj.gymAdmin);
        
        var command = new UpdateGymPassProductCommand(
            product.Id,
            name,
            description,
            totalUses,
            daysAfterExpiring,
            new Money(moneyAmount, moneyCurrency));
        
        var result =  await SendAsync(command);
        result.Succeeded.ShouldBeTrue();
        result.Value.ShouldNotBeNull();

        var productDto = result.Value;
        
        productDto.Id.ShouldBe(product.Id);
        productDto.Name.ShouldBe(name);
        productDto.Description.ShouldBe(name);
        productDto.TotalUses.ShouldBe(totalUses);
        productDto.DaysAfterExpiring.ShouldBe(daysAfterExpiring);
        productDto.Price.Amount.ShouldBe(moneyAmount);
        productDto.Price.Currency.ShouldBe(moneyCurrency);
    }
}
