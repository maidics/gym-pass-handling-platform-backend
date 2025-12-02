using FitPass.Domain.Entities;
using FitPass.Domain.Enums;
using FitPass.Domain.ValueObjects;
using NUnit.Framework;
using FitPass.Application.Gyms.DTOs;
using Shouldly;
using FitPass.Domain.Entities.Payment;
using FitPass.Application.GymPassProducts.DTOs;

namespace FitPass.Application.UnitTests.Mappings;

public class GymMapTests
{
    [Test]
    public void ShouldMapToDto()
    {
        var gym = new Gym
        {
            Id = "id",
            Name = "name",
            Address = new Address("line1", "line2", "city", null, "postalCode", "HU"),
            Status = GymStatus.Inactive,
            Tier = GymTier.Elite,
            PassProducts =
            [
                GymPassProduct.SingleUse("id", "Single Use Pass", "Description", true, new Money(1000, "huf")),
            ]
        };

        var dto = gym.MapToDto();

        dto.ShouldSatisfyAllConditions(
            () => dto.Name.ShouldBe(gym.Name),
            () => dto.Address.ShouldBeEquivalentTo(new Address("line1", "line2", "city", null, "postalCode", "HU")),
            () => dto.Status.ShouldBe(GymStatus.Inactive),
            () => dto.Tier.ShouldBe(GymTier.Elite),
            () => dto.PassProducts.ShouldBeEquivalentTo(new List<GymPassProductDto>(gym.PassProducts.Select(x => x.MapToDto()))));
    }
}
