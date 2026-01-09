using FitPass.Domain.Entities.Payment;
using NUnit.Framework;
using FitPass.Application.TenantPaymentProfiles.DTOs;
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
            AccountStatus = TenantPaymentAccountStatus.Default(),
            LastUpdatedByOnPaymentProvidersSide = null,
            LastUpdatedOnPaymentProvidersSide = now,
            LastAccountLinkGeneratedOn = now,
            LastAccountLinkGeneratedBy = null,
            PaymentAccountId = "id"
        };

        var dto = profile.MapToDto();

        dto.ShouldSatisfyAllConditions(
            () => dto.GymId.ShouldBe("GymId"),
            () => dto.AccountStatus.ShouldBeEquivalentTo(profile.AccountStatus.MapToDto()),
            () => dto.LastUpdatedByOnPaymentProvidersSide.ShouldBeNull(),
            () => dto.LastUpdatedOnPaymentProvidersSide.ShouldBe(now),
            () => dto.LastAccountLinkGeneratedOn.ShouldBe(now),
            () => dto.LastAccountLinkGeneratedBy.ShouldBeNull());
    }
}
