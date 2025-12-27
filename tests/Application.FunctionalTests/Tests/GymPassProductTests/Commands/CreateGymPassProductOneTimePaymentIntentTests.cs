using FitPass.Application.Common.Models;
using FitPass.Application.FunctionalTests.TestData;
using FitPass.Application.GymPassProducts.Commands;
using FitPass.Domain.Constants;
using FitPass.Domain.Enums;
using FitPass.Domain.ValueObjects;

namespace FitPass.Application.FunctionalTests.Tests.GymPassProductTests.Commands;

using static Testing;

public class CreateGymPassProductOneTimePaymentIntentTests : BaseTestFixture
{
    [Test]
    public override void AuthorizeAttributeCheck()
    {
        ShouldRequireAuthorization<CreateGymPassProductOneTimePaymentIntentCommand>(Roles.User);
    }
    
    [Test]
    public async Task ShouldReturnNotFoundIfGymPassProductNotFound()
    {
        await RunAsDefaultUserAsync();

        var command = new CreateGymPassProductOneTimePaymentIntentCommand("gymPassProductId");

        var result = await SendAsync(command);
        result.Type.ShouldBe(ResultTypes.NotFound);
        result.Message.ShouldNotBeEmpty();
    }

    [Test]
    public async Task ShouldReturnBusinessRuleViolationIfGymPassProductIsNotActive()
    {
        var obj = await TestEntityBuilder.BuildGymWithTenantPaymentProfileAsync(gymPassProductActive: false);
        
        await RunAsDefaultUserAsync();

        var command = new CreateGymPassProductOneTimePaymentIntentCommand(obj.gymPassProduct.Id);
        
        var result = await SendAsync(command);
        result.Type.ShouldBe(ResultTypes.BusinessRuleViolation);
        result.Message.ShouldNotBeEmpty();
    }

    [TestCase(GymStatus.Inactive)]
    [TestCase(GymStatus.Suspended)]
    public async Task ShouldReturnBusinessRuleViolationIfGymIsNotActive(GymStatus gymStatus)
    {
        var obj = await TestEntityBuilder.BuildGymWithTenantPaymentProfileAsync(gymStatus);

        await RunAsDefaultUserAsync();
        
        var command = new CreateGymPassProductOneTimePaymentIntentCommand(obj.gymPassProduct.Id);
        
        var result = await SendAsync(command);
        result.Type.ShouldBe(ResultTypes.BusinessRuleViolation);
        result.Message.ShouldNotBeEmpty();
    }

    [TestCase(PassType.SingleUse, 1, null)]
    [TestCase(PassType.MultiUse, 10, null)]
    [TestCase(PassType.Unlimited, null, 10)]
    public async Task ShouldCreatePaymentIntent(PassType passType, int? totalUses, int? daysAfterExpiring)
    {
        var obj = await TestEntityBuilder.BuildGymWithTenantPaymentProfileAsync();

        await RunAsDefaultUserAsync();

        var command = new CreateGymPassProductOneTimePaymentIntentCommand(obj.gymPassProduct.Id);
        
        var result = await SendAsync(command);
        result.Succeeded.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        
        var paymentIntent = result.Value;
        paymentIntent.ClientSecret.ShouldNotBeNullOrEmpty();
        paymentIntent.TenantPaymentAccountId.ShouldBe(obj.tenantPaymentProfile.PaymentAccountId);
    }
}
