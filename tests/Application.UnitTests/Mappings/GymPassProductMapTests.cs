using FitPass.Application.GymPassProducts.DTOs;
using FitPass.Domain.Entities;
using FitPass.Domain.Entities.Payment;
using FitPass.Domain.Enums;
using FitPass.Domain.ValueObjects;
using NUnit.Framework;
using Shouldly;

namespace FitPass.Application.UnitTests.Mappings;

public class GymPassProductMapTests
{
    [Test]
    public void ShouldMapToDto()
    {
        var product = GymPassProduct.SingleUse(
            "GymId",
            "name",
            "description",
            true,
            new Money(10, CurrencyCode.USD)
        );

        var dto = product.MapToDto();

        dto.ShouldSatisfyAllConditions(
            () => dto.Id.ShouldBe(product.Id),
            () => dto.GymId.ShouldBe("GymId"),
            () => dto.Name.ShouldBe("name"),
            () => dto.Description.ShouldBe("description"),
            () => dto.Type.ShouldBe(PassType.SingleUse),
            () => dto.TotalUses.ShouldBe(1),
            () => dto.DaysAfterExpiring.ShouldBeNull(),
            () => dto.IsActive.ShouldBeTrue(),
            () => dto.Price.ShouldBeEquivalentTo(new Money(10, CurrencyCode.USD))
        );
    }
}
