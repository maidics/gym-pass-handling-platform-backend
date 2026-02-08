using FitPass.Application.Common.Models;
using FitPass.Application.FunctionalTests.Common.Constants;
using FitPass.Application.FunctionalTests.Common.Extensions;

using FitPass.Application.FunctionalTests.TestData;
using FitPass.Application.PaymentIntents.Commands;
using FitPass.Domain.Constants;
using FitPass.Domain.Enums;

namespace FitPass.Application.FunctionalTests.Tests.PaymentIntents.Commands;

using static Testing;

public class CreateOneTimePaymentIntentForGymPassProductTests : BaseTestFixture
{
    [Test]
    public override void AuthorizeAttributeCheck()
    {
        ShouldRequireAuthorization<CreateOneTimePaymentIntentForGymPassProductCommand>(Roles.User);
    }

    [Test]
    public async Task ShouldReturnNotFoundIfGymPassProductNotFound()
    {
        await RunAsDefaultUserAsync();

        var command = new CreateOneTimePaymentIntentForGymPassProductCommand("id");

        var result = await SendAsync(command);
        result.ShouldBeFailed(ResultTypes.NotFound);
    }

    [Test]
    public async Task ShouldReturnBusinessRuleViolationIfGymPassProductIsNotActive()
    {
        var obj = await TestEntityBuilder.BuildGymWithTenantPaymentProfileAsync(
            gymPassProductActive: false
        );

        await RunAsDefaultUserAsync();

        var command = new CreateOneTimePaymentIntentForGymPassProductCommand(obj.gymPassProduct.Id);

        var result = await SendAsync(command);
        result.ShouldBeFailed(ResultTypes.BusinessRuleViolation);
    }

    [TestCase(GymStatus.Inactive)]
    [TestCase(GymStatus.Suspended)]
    public async Task ShouldReturnBusinessRuleViolationIfGymIsNotActive(GymStatus gymStatus)
    {
        var obj = await TestEntityBuilder.BuildGymWithTenantPaymentProfileAsync(gymStatus);

        await RunAsDefaultUserAsync();

        var command = new CreateOneTimePaymentIntentForGymPassProductCommand(obj.gymPassProduct.Id);

        var result = await SendAsync(command);
        result.ShouldBeFailed(ResultTypes.BusinessRuleViolation);
    }

    [TestCase(PassType.SingleUse, 1, null)]
    [TestCase(PassType.MultiUse, 10, null)]
    [TestCase(PassType.Unlimited, null, 10)]
    public async Task ShouldCreatePaymentIntent(
        PassType passType,
        int? totalUses,
        int? daysAfterExpiring
    )
    {
        var obj = await TestEntityBuilder.BuildGymWithTenantPaymentProfileAsync();

        await RunAsDefaultUserAsync();

        var command = new CreateOneTimePaymentIntentForGymPassProductCommand(obj.gymPassProduct.Id);

        var result = await SendAsync(command);
        result.Succeeded.ShouldBeTrue();

        var paymentIntent = result.Value;
        paymentIntent.ShouldNotBeNull();
        paymentIntent.ClientSecret.ShouldNotBeNullOrEmpty();
        paymentIntent.ClientSecret.ShouldStartWith(StripePrefixes.PaymentIntentId);
        paymentIntent.TenantPaymentAccountId.ShouldBe(obj.tenantPaymentProfile.PaymentAccountId);
        paymentIntent.TenantPaymentAccountId.ShouldStartWith(StripePrefixes.AccountId);
    }
}
