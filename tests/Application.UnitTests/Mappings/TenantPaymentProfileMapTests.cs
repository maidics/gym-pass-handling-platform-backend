using FitPass.Application.TenantPaymentProfiles.DTOs;
using FitPass.Domain.Entities.Payment;
using NUnit.Framework;
using Shouldly;

namespace FitPass.Application.UnitTests.Mappings;

public class TenantPaymentProfileMapTests
{
    [Test]
    public void ShouldMapToDto()
    {
        var now = DateTimeOffset.UtcNow;

        var profile = new TenantPaymentProfile
        {
            GymId = "GymId",
            LastAccountLinkGeneratedOn = now,
            LastAccountLinkGeneratedBy = null,
            PaymentAccountId = "id",
        };

        var dto = profile.MapToDto();

        dto.ShouldSatisfyAllConditions(
            () => dto.GymId.ShouldBe("GymId"),
            () => dto.LastAccountLinkGeneratedOn.ShouldBe(now),
            () => dto.LastAccountLinkGeneratedBy.ShouldBeNull()
        );
    }
}
