using FitPass.Application.Common.Models;
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
        ShouldRequireAuthorization<GetTenantPaymentProfileQuery>(Roles.GymAdministrator);
    }
    
    [Test]
    public async Task ShouldReturnNotFoundIfTenantPaymentProfileNotFound()
    {
        var obj = await TestEntityBuilder.BuildGymAsync();

        await RunAsUserAsync(obj.gymAdmin);

        var result = await SendAsync(new GetTenantPaymentProfileQuery());
        result.Type.ShouldBe(ResultTypes.NotFound);
        result.Message.ShouldContain($"{nameof(TenantPaymentProfile)} not found");
    }
    
    [Test]
    public async Task ShouldReturnTenantPaymentProfile()
    {
        var obj = await TestEntityBuilder.BuildGymWithTenantPaymentProfileAync();

        await RunAsUserAsync(obj.gymAdmin);

        var result = await SendAsync(new GetTenantPaymentProfileQuery());
        result.Succeeded.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.ShouldBeEquivalentTo(obj.tenantPaymentProfile.MapToDto());
    }
}
