using System;
using FitPass.Domain.Entities.Payment;
using NUnit.Framework;
using FitPass.Application.TenantPaymentProfiles.DTOs;
using Shouldly;

namespace FitPass.Application.UnitTests.Mappings;

public class TenantPaymentAccountStatusMapTests
{
    [Test]
    public void ShouldMapToDto()
    {
        var status = new TenantPaymentAccountStatus
        {
            ChargesEnabled = true,
            DetailsSubmitted = false,
            PayoutsEnabled = false,
            RequirementsDue = ["1", "2"],
            RequirementsEventuallyDue = ["1", "2"]
        };

        var dto = status.MapToDto();

        dto.ShouldSatisfyAllConditions(
            () => dto.ChargesEnabled.ShouldBeTrue(),
            () => dto.DetailsSubmitted.ShouldBeFalse(),
            () => dto.PayoutsEnabled.ShouldBeFalse(),
            () => dto.RequirementsDue.ShouldBeEquivalentTo(status.RequirementsDue),
            () => dto.RequirementsEventuallyDue.ShouldBeEquivalentTo(status.RequirementsEventuallyDue));
    }
}
