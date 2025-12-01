using FitPass.Domain.ValueObjects;
using NUnit.Framework;
using Shouldly;

namespace FitPass.Domain.UnitTests.ValueObjects;

public class CountryTests
{
    [TestCase("HU")]
    [TestCase("DE")]
    public void ShouldReturnCountryByValidAlpha2(string alpha2)
    {
        var country = Country.GetByAlpha2(alpha2);

        country.ShouldNotBeNull();
        country.Alpha2.ShouldBe(alpha2);
        country.Alpha3.ShouldNotBeNullOrEmpty();
        country.Numeric.ShouldNotBeNullOrEmpty();
    }

    [TestCase("")]
    [TestCase("Invalid")]
    [TestCase("XX")]
    public void ShouldNotReturnCountryByInvalidAlpha2(string alpha2)
    {
        var country = Country.GetByAlpha2(alpha2);

        country.ShouldBeNull();
    }

    [TestCase("HU")]
    [TestCase("DE")]
    public void ShouldReturnTrueForEqualCountries(string alpha2)
    {
        var country1 = Country.GetByAlpha2(alpha2);
        var country2 = Country.GetByAlpha2(alpha2);

        country1.ShouldNotBeNull();
        country2.ShouldNotBeNull();

        (country1 == country2).ShouldBeTrue();
    }
}
