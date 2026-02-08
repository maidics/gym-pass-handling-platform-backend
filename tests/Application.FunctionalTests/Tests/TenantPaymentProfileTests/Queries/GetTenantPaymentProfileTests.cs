using FitPass.Application.Common.Models;
using FitPass.Application.FunctionalTests.Common.Extensions;

using FitPass.Application.FunctionalTests.TestData;
using FitPass.Application.TenantPaymentProfiles.DTOs;
using FitPass.Application.TenantPaymentProfiles.Queries;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities.Payment;

namespace FitPass.Application.FunctionalTests.Tests.TenantPaymentProfileTests.Queries;

using static Testing;

public class GetTenantPaymentProfileTests : BaseTestFixture
{
    [Test]
    public override void AuthorizeAttributeCheck()
    {
        ShouldRequireAuthorization<GetMyTenantPaymentProfileQuery>(Roles.GymAdministrator);
    }

    [Test]
    public async Task ShouldReturnBusinessRuleViolationIfGymHasNoPaymentProfile()
    {
        var obj = await TestEntityBuilder.BuildGymAsync();

        await RunAsUserAsync(obj.gymAdmin);

        var query = new GetMyTenantPaymentProfileQuery();

        var result = await SendAsync(query);
        result.ShouldBeFailed(ResultTypes.BusinessRuleViolation);
    }

    [Test]
    public async Task ShouldReturnTenantPaymentProfile()
    {
        var obj = await TestEntityBuilder.BuildGymWithTenantPaymentProfileAsync();

        await RunAsUserAsync(obj.gymAdmin);

        var result = await SendAsync(new GetMyTenantPaymentProfileQuery());
        result.ShouldBeSuccessful();

        var dto = result.Value;
        dto.GymId.ShouldBe(obj.gym.Id);
    }
}
