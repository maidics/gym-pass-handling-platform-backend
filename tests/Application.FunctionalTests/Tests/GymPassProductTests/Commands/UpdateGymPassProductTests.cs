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
using FitPass.Infrastructure.Identity;

namespace FitPass.Application.FunctionalTests.Tests.GymPassProductTests.Commands;

using static Testing;

public class UpdateGymPassProductTests : BaseTestFixture
{
    [Test]
    public override void AuthorizeAttributeCheck()
    {
        ShouldRequireAuthorization<UpdateGymPassProductCommand>(Roles.GymAdministrator);
    }

    [TestCase("id", PassType.SingleUse, "", "Product Description", 10, CurrencyCode.USD, 1, null)]
    [TestCase("id", PassType.SingleUse, "Product Name", "", 10, CurrencyCode.USD, 1, null)]
    [TestCase(
        "id",
        PassType.SingleUse,
        "Product Name",
        "Product Description",
        10,
        CurrencyCode.HUF,
        1,
        null
    )]
    [TestCase(
        "id",
        PassType.SingleUse,
        "Product Name",
        "Product Description",
        10,
        CurrencyCode.EUR,
        2,
        null
    )]
    [TestCase(
        "id",
        PassType.SingleUse,
        "Product Name",
        "Product Description",
        10,
        CurrencyCode.EUR,
        1,
        10
    )]
    [TestCase(
        "id",
        PassType.MultiUse,
        "Product Name",
        "Product Description",
        10,
        CurrencyCode.EUR,
        1,
        null
    )]
    [TestCase(
        "id",
        PassType.MultiUse,
        "Product Name",
        "Product Description",
        10,
        CurrencyCode.EUR,
        1,
        10
    )]
    [TestCase(
        "id",
        PassType.Unlimited,
        "Product Name",
        "Product Description",
        10,
        CurrencyCode.EUR,
        1,
        null
    )]
    [TestCase(
        "id",
        PassType.Unlimited,
        "Product Name",
        "Product Description",
        10,
        CurrencyCode.EUR,
        null,
        null
    )]
    public async Task ShouldThrowIfParametersAreInvalid(
        string gymPassProductId,
        PassType type,
        string name,
        string description,
        decimal priceAmount,
        CurrencyCode priceCurrency,
        int? totalUses,
        int? daysAfterExpiring
    )
    {
        await RunAsGymEmployeeAsync(Roles.GymAdministrator);

        var command = new UpdateGymPassProductCommand(
            gymPassProductId,
            type,
            name,
            description,
            new Money(priceAmount, priceCurrency),
            totalUses,
            daysAfterExpiring
        );

        await ShouldThrowIfParametersAreInvalidAsync(command);
    }

    [Test]
    public async Task ShouldReturnBusinessRuleViolationIfTheGymIsSuspended()
    {
        var obj = await TestEntityBuilder.BuildGymAsync(GymStatus.Suspended);

        await RunAsUserAsync(obj.gymAdmin);

        var command = new UpdateGymPassProductCommand(
            "gymPassProductId",
            PassType.SingleUse,
            "Updated Name",
            "Updated Description",
            new Money(10, CurrencyCode.USD),
            1,
            null
        );

        var result = await SendAsync(command);
        result.ShouldBeFailed(ResultTypes.BusinessRuleViolation);
    }

    [Test]
    public async Task ShouldThrowIfGymHasNoPaymentProfile()
    {
        var obj = await TestEntityBuilder.BuildGymAsync();

        await RunAsUserAsync(obj.gymAdmin);

        var command = new UpdateGymPassProductCommand(
            obj.singleUsePass.Id,
            PassType.SingleUse,
            "Updated Name",
            "Updated Description",
            new Money(10, CurrencyCode.USD),
            1,
            null
        );

        await Should.ThrowAsync<ArgumentNullException>(SendAsync(command));
    }

    [Test]
    public async Task ShouldReturnNotFoundIfGymPassProductNotFound()
    {
        var obj = await TestEntityBuilder.BuildGymWithTenantPaymentProfileAsync();

        await RunAsUserAsync(obj.gymAdmin);

        var command = new UpdateGymPassProductCommand(
            "id",
            PassType.SingleUse,
            "Updated Name",
            "Updated Description",
            new Money(10, CurrencyCode.USD),
            1,
            null
        );

        var result = await SendAsync(command);
        result.ShouldBeFailed(ResultTypes.NotFound);
    }

    [Test]
    public async Task ShouldThrowIfProductHasNoPaymentIdentity()
    {
        var obj = await TestEntityBuilder.BuildGymAsync();

        await RunAsUserAsync(obj.gymAdmin);

        var command = new UpdateGymPassProductActiveStatusCommand(obj.gymPassProduct.Id, false);

        await Should.ThrowAsync<ArgumentNullException>(SendAsync(command));
    }

    [TestCase(
        PassType.SingleUse,
        1,
        null,
        "Updated Name",
        "Updated Description",
        1,
        null,
        49.99,
        CurrencyCode.EUR
    )]
    [TestCase(
        PassType.MultiUse,
        10,
        null,
        "Updated Name",
        "Updated Description",
        20,
        null,
        79.99,
        CurrencyCode.EUR
    )]
    [TestCase(
        PassType.Unlimited,
        null,
        30,
        "Updated Name",
        "Updated Description",
        null,
        60,
        99.99,
        CurrencyCode.USD
    )]
    public async Task ShouldUpdateGymPassProduct(
        PassType passType,
        int? passTotalUses,
        int? passDaysAfterExpiring,
        string newName,
        string newDescription,
        int? newTotalUses,
        int? newDaysAfterExpiring,
        decimal newMoneyAmount,
        CurrencyCode newMoneyCurrency
    )
    {
        var obj = await TestEntityBuilder.BuildGymWithTenantPaymentProfileAsync();
        var product = await TestEntityBuilder.BuildGymPassProductWithPaymentProfile(
            obj.gymAdmin,
            new Money(10, CurrencyCode.USD),
            type: passType,
            totalUses: passTotalUses,
            daysAfterExpiring: passDaysAfterExpiring
        );

        var paymentIdentity = product.PaymentIdentity;

        string oldProductId = paymentIdentity.ProductId;
        string oldPriceId = paymentIdentity.PriceId;

        await RunAsUserAsync(obj.gymAdmin);

        var command = new UpdateGymPassProductCommand(
            product.Id,
            passType,
            newName,
            newDescription,
            new Money(newMoneyAmount, newMoneyCurrency),
            newTotalUses,
            newDaysAfterExpiring
        );

        var result = await SendAsync(command);
        result.ShouldBeSuccessful();

        var productDto = result.Value;
        productDto.ShouldNotBeNull();
        productDto.Id.ShouldBe(product.Id);

        var updatedProduct = await FindAsync<GymPassProduct>(product.Id);
        updatedProduct.ShouldNotBeNull();
        updatedProduct.Name.ShouldBe(newName);
        updatedProduct.Description.ShouldBe(newDescription);
        updatedProduct.TotalUses.ShouldBe(newTotalUses);
        updatedProduct.DaysAfterExpires.ShouldBe(newDaysAfterExpiring);
        updatedProduct.Price.Amount.ShouldBe(newMoneyAmount);
        updatedProduct.Price.Currency.ShouldBe(newMoneyCurrency);

        var updatedPaymentIdentity = await GetFirstAsync<ProductPaymentIdentity>();
        updatedPaymentIdentity.ShouldNotBeNull();
        updatedPaymentIdentity.PriceId.ShouldNotBe(oldPriceId);
        updatedPaymentIdentity.PriceId.ShouldStartWith(StripePrefixes.PriceId);
        updatedPaymentIdentity.ProductId.ShouldBe(oldProductId);
        updatedPaymentIdentity.ProductId.ShouldStartWith(StripePrefixes.ProductId);
    }

    [Test]
    public async Task ShouldNotUpdatePriceIfTheSame()
    {
        var obj = await TestEntityBuilder.BuildGymWithTenantPaymentProfileAsync();

        var product = await TestEntityBuilder.BuildGymPassProductWithPaymentProfile(
            obj.gymAdmin,
            new Money(10, CurrencyCode.USD)
        );

        var paymentIdentity = product.PaymentIdentity;

        await RunAsUserAsync(obj.gymAdmin);

        var command = new UpdateGymPassProductCommand(
            product.Id,
            product.Type,
            product.Name,
            product.Description,
            product.Price,
            product.TotalUses,
            product.DaysAfterExpires
        );

        var result = await SendAsync(command);
        result.Succeeded.ShouldBeTrue();

        var productDto = result.Value;
        productDto.ShouldNotBeNull();

        var notUpdatedProduct = await FindAsync<GymPassProduct>(
            [productDto.Id],
            x => x.PaymentIdentity
        );

        notUpdatedProduct.ShouldNotBeNull();

        var notUpdatedPaymentIdentity = notUpdatedProduct.PaymentIdentity;
        notUpdatedPaymentIdentity.ShouldNotBeNull();
        notUpdatedPaymentIdentity.PriceId.ShouldBe(paymentIdentity.PriceId);
        notUpdatedPaymentIdentity.ProductId.ShouldBe(paymentIdentity.ProductId);

        productDto.Id.ShouldBe(product.Id);
        productDto.Price.Amount.ShouldBe(product.Price.Amount);
        productDto.Price.Currency.ShouldBe(product.Price.Currency);
    }
}
