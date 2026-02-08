using FitPass.Application.Common.Models;
using FitPass.Application.FunctionalTests.Common.Constants;
using FitPass.Application.FunctionalTests.Common.Extensions;

using FitPass.Application.FunctionalTests.TestData;
using FitPass.Application.TenantPaymentProfiles.Commands;
using FitPass.Application.TenantPaymentProfiles.DTOs;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities.Payment;

namespace FitPass.Application.FunctionalTests.Tests.TenantPaymentProfileTests.Commands;

using static Testing;

public class CreateTenantPaymentProfileTests : BaseTestFixture
{
    [Test]
    public override void AuthorizeAttributeCheck()
    {
        ShouldRequireAuthorization<CreateTenantPaymentProfileCommand>(Roles.GymAdministrator);
    }

    [Test]
    public async Task ShouldReturnForbiddenIfPaymentProfileAlreadyExists()
    {
        var obj = await TestEntityBuilder.BuildGymWithTenantPaymentProfileAsync();

        await RunAsUserAsync(obj.gymAdmin);

        var command = new CreateTenantPaymentProfileCommand(
            "accountholder@test.com",
            "Test Business"
        );

        var result = await SendAsync(command);
        result.ShouldBeFailed(ResultTypes.Forbidden);
    }

    [Test]
    public async Task ShouldCreatePaymentProfileAndReturnOnboardingUrl()
    {
        var obj = await TestEntityBuilder.BuildGymAsync();

        await RunAsUserAsync(obj.gymAdmin);

        var command = new CreateTenantPaymentProfileCommand("test@localhost.com", "Test Business");

        var result = await SendAsync(command);
        result.ShouldBeSuccessful();

        var dto = result.Value;
        dto.Type.ShouldBe(PaymentProviderLinkType.AccountLink);
        dto.Url.ShouldNotBeNullOrEmpty();

        var paymentProfile = await GetFirstAsync<TenantPaymentProfile>();
        paymentProfile.ShouldNotBeNull();
        paymentProfile.GymId.ShouldBe(obj.gym.Id);
        paymentProfile.PaymentAccountId.ShouldNotBeNullOrEmpty();
        paymentProfile.PaymentAccountId.ShouldStartWith(StripePrefixes.AccountId);
    }
}
