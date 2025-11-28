using FitPass.Domain.ValueObjects;
using NUnit.Framework;
using Shouldly;

namespace FitPass.Domain.UnitTests.ValueObjects;

public class AddressTests
{
    [TestCase("Fehér utca 2", "B/34", "Veszprém", "Győr-Moson-Sopron megye", "8200", "HU")]
    [TestCase("Fehér utca 2", "B/34", "Veszprém", null, "8200", "HU")]
    public void ShouldReturnAddress(string line1, string line2, string city, string? state, string postalCode, string countryAlpha2)
    {
        var address = new Address(
            line1,
            line2,
            city,
            state,
            postalCode,
            countryAlpha2);

        address.ShouldSatisfyAllConditions(
            () => address.Line1.ShouldBe(line1),
            () => address.Line2.ShouldBe(line2),
            () => address.City.ShouldBe(city),
            () => address.State.ShouldBe(state),
            () => address.PostalCode.ShouldBe(postalCode),
            () => address.CountryAlpha2.ShouldBe(countryAlpha2)
        );
    }

    [TestCase("", "", "", null, "", "")]
    public void ShouldThrowForInvalidParameters(string line1, string line2, string city, string? state, string postalCode, string countryAlpha2)
    {
        var construct = () => new Address(line1, line2, city, state, postalCode, countryAlpha2);

        Should.Throw<ArgumentException>(construct);
    }
}
