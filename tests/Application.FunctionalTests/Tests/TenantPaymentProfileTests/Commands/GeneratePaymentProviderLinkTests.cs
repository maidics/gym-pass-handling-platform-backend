using FitPass.Application.Common.Models;
using FitPass.Application.FunctionalTests.Common.Extensions;

using FitPass.Application.FunctionalTests.TestData;
using FitPass.Application.TenantPaymentProfiles.Commands;
using FitPass.Application.TenantPaymentProfiles.DTOs;
using FitPass.Domain.Constants;

namespace FitPass.Application.FunctionalTests.Tests.TenantPaymentProfileTests.Commands;

using static Testing;

public class GeneratePaymentProviderLinkTests : BaseTestFixture
{
    [Test]
    public override void AuthorizeAttributeCheck()
    {
        ShouldRequireAuthorization<GeneratePaymentProviderLinkCommand>(Roles.GymAdministrator);
    }

    [Test]
    public async Task ShouldReturnBusinessRuleViolationIfGymHasNoPaymentProfile()
    {
        var obj = await TestEntityBuilder.BuildGymAsync();

        await RunAsUserAsync(obj.gymAdmin);

        var command = new GeneratePaymentProviderLinkCommand(PaymentProviderLinkType.AccountLink);

        var result = await SendAsync(command);
        result.ShouldBeFailed(ResultTypes.BusinessRuleViolation);
    }

    [TestCase(PaymentProviderLinkType.AccountLink)]
    [TestCase(PaymentProviderLinkType.LoginLink)]
    public async Task ShouldReturnPaymentProviderLoginLink(PaymentProviderLinkType linkType)
    {
        var obj = await TestEntityBuilder.BuildGymWithTenantPaymentProfileAsync();

        await RunAsUserAsync(obj.gymAdmin);

        var command = new GeneratePaymentProviderLinkCommand(linkType);

        var result = await SendAsync(command);
        result.ShouldBeSuccessful();

        var dto = result.Value;
        dto.Type.ShouldBe(linkType);
        dto.Url.ShouldNotBeNullOrEmpty();
    }
}
