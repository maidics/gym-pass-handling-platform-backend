using FitPass.Application.Common.Models;
using FitPass.Application.FunctionalTests.TestData;
using FitPass.Application.TenantPaymentProfiles.Commands;
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
        var obj = await TestEntityBuilder.BuildGymAsync();

        var paymentProfile = new TenantPaymentProfile
        {
            GymId = obj.gym.Id,
            PaymentAccountId = "existing_account_id",
            AccountStatus = TenantPaymentAccountStatus.Default()
        };

        await AddAsync(paymentProfile);

        await RunAsUserAsync(obj.gymAdmin);

        var command = new CreateTenantPaymentProfileCommand("test@localhost", "Test Business");

        var result = await SendAsync(command);
        result.Type.ShouldBe(ResultTypes.Forbidden);
        result.Message.ShouldNotBeEmpty();
    }

    [Test]
    public async Task ShouldCreatePaymentProfileAndReturnOnboardingUrl()
    {
        var obj = await TestEntityBuilder.BuildGymAsync();

        await RunAsUserAsync(obj.gymAdmin);

        var command = new CreateTenantPaymentProfileCommand("test@localhost", "Test Business");

        var result = await SendAsync(command);

        result.Succeeded.ShouldBeTrue();
        result.Value.url.ShouldNotBeNullOrEmpty();
        result.Value.expiration.ShouldBeGreaterThan(DateTimeOffset.UtcNow);

        var paymentProfile = await GetFirstAsync<TenantPaymentProfile>();
        paymentProfile.ShouldNotBeNull();
        paymentProfile.GymId.ShouldBe(obj.gym.Id);
        paymentProfile.PaymentAccountId.ShouldNotBeNullOrEmpty();
    }
}
