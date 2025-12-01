using FitPass.Domain.Entities;
using NUnit.Framework;
using FitPass.Application.GymMemberships.DTOs;
using FitPass.Application.GymMembershipPasses.DTOs;
using Shouldly;
using FitPass.Domain.Enums;

namespace FitPass.Application.UnitTests.Mappings;

public class GymMembershipDtoTests
{
    [Test]
    public void ShouldMapToDto()
    {
        var gymMembership = new GymMembership
        {
            Id = "id",
            UserId = "userId",
            GymId = "gymId",
            Status = GymMembershipStatus.Banned,
            Passes = 
            [
                new GymMembershipPass 
                {
                    GymMembershipId = "id",
                    UserId = "userId",
                    Type = PassType.SingleUse,
                    TotalUses = 1,
                    RemainingUses = 1,
                    ExpirationDate = null
                }
            ]
        };

        var dto = gymMembership.MapToDto();

        dto.ShouldSatisfyAllConditions(
            () => dto.Id.ShouldBe("id"),
            () => dto.UserId.ShouldBe("userId"),
            () => dto.GymId.ShouldBe("gymId"),
            () => dto.Status.ShouldBe(GymMembershipStatus.Banned),
            () => dto.Passes.Count.ShouldBe(1),
            () => dto.Passes.First().ShouldBeEquivalentTo(gymMembership.Passes.First().MapToDto()));
    }
}
