using FitPass.Application.GymMembershipPasses.DTOs;
using FitPass.Domain.Entities;
using FitPass.Domain.Enums;
using NUnit.Framework;
using Shouldly;

namespace FitPass.Application.UnitTests.Mappings;

public class GymMembershipPassDtoTests
{
    [Test]
    public void ShouldMapToDto()
    {
        var pass = new GymMembershipPass
        {
            Id = "id",
            GymMembershipId = "membershipId",
            UserId = "userId",
            Type = PassType.SingleUse,
            TotalUses = 1,
            RemainingUses = 1,
            ExpirationDate = null
        };

        var dto = pass.MapToDto();

        dto.ShouldSatisfyAllConditions(
            () => dto.Id.ShouldBe("id"),
            () => dto.GymMembershipId.ShouldBe("membershipId"),
            () => dto.Type.ShouldBe(pass.Type),
            () => dto.TotalUses.ShouldBe(1),
            () => dto.RemainingUses.ShouldBe(1),
            () => dto.ExpirationDate.ShouldBeNull());
    }
}
