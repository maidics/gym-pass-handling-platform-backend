using FitPass.Application.Common.Models;
using FitPass.Application.FunctionalTests.Common.Constants;
using FitPass.Application.FunctionalTests.Common.Extensions;
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

    [TestCase(PassType.SingleUse, 1, null, 0, CurrencyCode.HUF)]
    [TestCase(PassType.SingleUse, 1, null, 125, CurrencyCode.HUF)]
    [TestCase(PassType.SingleUse, 1, null, 249, CurrencyCode.HUF)]
    [TestCase(PassType.SingleUse, 1, null, 0, CurrencyCode.USD)]
    [TestCase(PassType.SingleUse, 1, null, 0.9, CurrencyCode.USD)]
    [TestCase(PassType.SingleUse, 1, null, 0, CurrencyCode.EUR)]
    [TestCase(PassType.SingleUse, 1, null, 0.9, CurrencyCode.EUR)]
    [TestCase(PassType.SingleUse, null, 10, 250, CurrencyCode.HUF)]
    [TestCase(PassType.SingleUse, null, null, 250, CurrencyCode.HUF)]
    [TestCase(PassType.SingleUse, 0, null, 250, CurrencyCode.HUF)]
    [TestCase(PassType.SingleUse, 2, null, 250, CurrencyCode.HUF)]
    [TestCase(PassType.MultiUse, null, null, 250, CurrencyCode.HUF)]
    [TestCase(PassType.MultiUse, null, 1, 250, CurrencyCode.HUF)]
    [TestCase(PassType.MultiUse, 0, null, 250, CurrencyCode.HUF)]
    [TestCase(PassType.MultiUse, 1, null, 250, CurrencyCode.HUF)]
    [TestCase(PassType.Unlimited, null, null, 250, CurrencyCode.HUF)]
    [TestCase(PassType.Unlimited, 0, null, 250, CurrencyCode.HUF)]
    [TestCase(PassType.Unlimited, 1, null, 250, CurrencyCode.HUF)]
    [TestCase(PassType.Unlimited, 99, null, 250, CurrencyCode.HUF)]
    public async Task ShouldThrowIfParametersAreInvalid(
        PassType type,
        int? totalUses,
        int? daysAfterExpiring,
        decimal priceAmount,
        CurrencyCode priceCurrency
    )
    {
        await RunAsGymEmployeeAsync(Roles.GymAdministrator);

        var command = new CreateGymPassProductCommand(
            "Product Name",
            "Product Description",
            PassType.SingleUse,
            null,
            null,
            true,
            new Money(priceAmount, priceCurrency)
        );

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
            new Money(1000, CurrencyCode.HUF)
        );

        var result = await SendAsync(command);
        result.ShouldBeFailed(ResultTypes.BusinessRuleViolation);
    }

    [TestCase(GymStatus.Suspended)]
    [TestCase(GymStatus.Inactive)]
    public async Task ShouldReturnBusinessRuleViolationIfGymStatusIsNotActive(GymStatus gymStatus)
    {
        var obj = await TestEntityBuilder.BuildGymAsync(gymStatus);

        await RunAsUserAsync(obj.gymAdmin);

        var command = new CreateGymPassProductCommand(
            "Test Product",
            "Test Description",
            PassType.SingleUse,
            1,
            null,
            false,
            new Money(10, CurrencyCode.USD)
        );

        var result = await SendAsync(command);
        result.ShouldBeFailed(ResultTypes.BusinessRuleViolation);
    }

    [TestCase(PassType.SingleUse, 1, null, 2, CurrencyCode.USD)]
    [TestCase(PassType.MultiUse, 2, null, 800, CurrencyCode.HUF)]
    [TestCase(PassType.MultiUse, 20, null, 22, CurrencyCode.EUR)]
    [TestCase(PassType.Unlimited, null, 1, 2, CurrencyCode.USD)]
    [TestCase(PassType.Unlimited, null, 120, 40000, CurrencyCode.HUF)]
    public async Task ShouldCreateGymPassProduct(
        PassType type,
        int? totalUses,
        int? daysAfterExpiring,
        decimal priceAmount,
        CurrencyCode priceCurrency
    )
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
            new Money(priceAmount, priceCurrency)
        );

        var result = await SendAsync(command);
        result.ShouldBeSuccessful();
        result.Value.ShouldNotBeNull();

        var gymPassProduct = await FindAsync<GymPassProduct>(
            [result.Value.Id],
            x => x.PaymentIdentity
        );

        gymPassProduct.ShouldNotBeNull();
        gymPassProduct.Type.ShouldBe(type);
        gymPassProduct.TotalUses.ShouldBe(totalUses);
        gymPassProduct.DaysAfterExpiring.ShouldBe(daysAfterExpiring);
        gymPassProduct.Price.Amount.ShouldBe(priceAmount);
        gymPassProduct.Price.Currency.ShouldBe(priceCurrency);

        var productIdentity = gymPassProduct.PaymentIdentity;
        productIdentity.ShouldNotBeNull();
        productIdentity.GymPassProductId.ShouldBe(gymPassProduct.Id);
        productIdentity.ProductId.ShouldNotBeNullOrEmpty();
        productIdentity.ProductId.ShouldStartWith(StripePrefixes.ProductId);
        productIdentity.PriceId.ShouldNotBeNullOrEmpty();
        productIdentity.PriceId.ShouldStartWith(StripePrefixes.PriceId);
    }
}
