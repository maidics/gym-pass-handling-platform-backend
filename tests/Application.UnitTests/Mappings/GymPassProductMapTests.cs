using FitPass.Domain.Entities;
using FitPass.Domain.Entities.Payment;
using FitPass.Domain.Enums;
using FitPass.Domain.ValueObjects;
using FitPass.Application.GymPassProducts.DTOs;
using NUnit.Framework;
using Shouldly;

namespace FitPass.Application.UnitTests.Mappings;

public class GymPassProductMapTests
{
    [Test]
    public void ShouldMapToDto()
    {
        var product = new GymPassProduct
        {
            Id = "id",
            GymId = "gymId",
            Name = "name",
            Description = "description",
            Type = PassType.MultiUse,
            TotalUses = 2,
            DaysAfterExpiring = null,
            IsActive = true,
            Price = Money.Zero("usd"),
            PaymentIdentity = new ProductPaymentIdentity
            {
                GymPassProductId = "id",
                PriceId = "priceId"
            }
        };

        var dto = product.MapToDto();

        dto.ShouldSatisfyAllConditions(
            () => dto.Id.ShouldBe("id"),
            () => dto.GymId.ShouldBe("gymId"),
            () => dto.Name.ShouldBe("name"),
            () => dto.Description.ShouldBe("description"),
            () => dto.Type.ShouldBe(PassType.MultiUse),
            () => dto.TotalUses.ShouldBe(2),
            () => dto.DaysAfterExpiring.ShouldBeNull(),
            () => dto.IsActive.ShouldBeTrue(),
            () => dto.Price.ShouldBeEquivalentTo(Money.Zero("usd")));
    }
}
