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

public class CreateGymPassProductTests : BaseTestFixture
{
    [Test]
    public override void AuthorizeAttributeCheck()
    {
        ShouldRequireAuthorization<CreateGymPassProductCommand>(Roles.GymAdministrator);
    }

    [TestCase("", "", PassType.SingleUse, 1, null, false)]
    [TestCase("Test Product", "Test Description", PassType.SingleUse, null, 10, false)]
    [TestCase("Test Product", "Test Description", PassType.SingleUse, null, null, false)]
    [TestCase("Test Product", "Test Description", PassType.SingleUse, 0, null, false)]
    [TestCase("Test Product", "Test Description", PassType.SingleUse, 2, null, false)]
    [TestCase("Test Product", "Test Description", PassType.MultiUse, null, null, false)]
    [TestCase("Test Product", "Test Description", PassType.MultiUse, null, 1, false)]
    [TestCase("Test Product", "Test Description", PassType.MultiUse, 0, null, false)]
    [TestCase("Test Product", "Test Description", PassType.MultiUse, 1, null, false)]
    [TestCase("Test Product", "Test Description", PassType.Unlimited, null, null, false)]
    [TestCase("Test Product", "Test Description", PassType.Unlimited, 0, null, false)]
    [TestCase("Test Product", "Test Description", PassType.Unlimited, 1, null, false)]
    [TestCase("Test Product", "Test Description", PassType.Unlimited, 99, null, false)]
    public async Task ShouldDenyInvalidParameters(
        string name, string description, PassType type, int? totalUses, int? daysAfterExpiring, bool isActive)
    {
        await RunAsGymEmployeeAsync(Roles.GymAdministrator);
        
        var command = new CreateGymPassProductCommand(
            string.Empty,
            string.Empty,
            PassType.SingleUse,
            null,
            null,
            true,
            Money.Usd(10));
        
        await ShouldThrowIfParametersAreInvalidAsync(command);
    }

    [Test]
    public async Task ShouldReturnBusinessRuleViolationIfGymHasNoTenantPaymentAccount()
    {
        var obj = await TestEntityBuilder.BuildGymAsync();

        await RunAsUserAsync(obj.gymAdmin);
        
        var command = new CreateGymPassProductCommand(
            "Test Product",
            "Test Description",
            PassType.SingleUse,
            1,
            null,
            true,
            new Money(1000, "huf"));
        
        var result = await SendAsync(command);
        result.Type.ShouldBe(ResultTypes.BusinessRuleViolation);
        result.Message.ShouldNotBeEmpty();
    }

    [Test]
    public async Task ShouldReturnBusinessRuleViolationIfGymPassProductIsNotActive()
    {
        var obj = await TestEntityBuilder.BuildGymAsync(GymStatus.Suspended);

        await RunAsUserAsync(obj.gymAdmin);
        
        var command = new CreateGymPassProductCommand(
            "Test Product",
            "Test Description",
            PassType.SingleUse,
            1,
            null,
            false,
            Money.Usd(10m));

        var result = await SendAsync(command);
        result.Type.ShouldBe(ResultTypes.BusinessRuleViolation);
        result.Message.ShouldNotBeEmpty();
    }

    [TestCase(PassType.SingleUse, 1, null)]
    [TestCase(PassType.MultiUse, 2, null)]
    [TestCase(PassType.MultiUse, 20, null)]
    [TestCase(PassType.Unlimited, null, 1)]
    [TestCase(PassType.Unlimited, null, 120)]
    public async Task ShouldCreateGymPassProduct(PassType type, int? totalUses, int? daysAfterExpiring)
    {
        var obj = await TestEntityBuilder.BuildGymWithTenantPaymentProfileAsync();

        await RunAsUserAsync(obj.gymAdmin);

        var command = new CreateGymPassProductCommand(
            "Test Product",
            "Test Description",
            type,
            totalUses,
            daysAfterExpiring,
            true,
            new Money(1000, "huf"));
        
        var result = await SendAsync(command);
        result.Succeeded.ShouldBeTrue();
        result.Value.ShouldNotBeNull();

        var gymPassProduct = await FindAsync<GymPassProduct>(result.Value.Id);
        gymPassProduct.ShouldNotBeNull();
        result.Value.AssertTo(gymPassProduct);

        var productIdentity = await GetFirstAsync<ProductPaymentIdentity>();
        productIdentity.ShouldNotBeNull();
        productIdentity.GymPassProductId.ShouldBe(gymPassProduct.Id);
        productIdentity.ProductId.ShouldNotBeNullOrEmpty();
        productIdentity.PriceId.ShouldNotBeNullOrEmpty();
    }
}
