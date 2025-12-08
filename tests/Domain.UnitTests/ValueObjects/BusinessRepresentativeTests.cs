using FitPass.Domain.ValueObjects;
using NUnit.Framework;
using Shouldly;

namespace FitPass.Domain.UnitTests.ValueObjects;

public class BusinessRepresentativeTests
{
    [Test]
    public void ShouldReturnBusinessRepresentative()
    {
        var phone = PhoneNumber.Create("+36201111111");
        var address = new Address("Fehér utca 2", "B/34", "Veszprém", "Győr-Moson-Sopron megye", "8200", "HU");

        var br = new BusinessRepresentative(
            "Lajos",
            "Kovács",
            "kovacs.lajos@localhost",
            phone,
            new DateOnly(1990, 10, 1),
            address,
            DateTimeOffset.UtcNow);

        br.ShouldSatisfyAllConditions(
            () => br.FirstName.ShouldBe("Lajos"),
            () => br.LastName.ShouldBe("Kovács"),
            () => br.Email.ShouldBe("kovacs.lajos@localhost"),
            () => br.Phone.ShouldBe(phone),
            () => br.DateOfBirth.ShouldBe(new DateOnly(1990, 10, 1)),
            () => br.Address.ShouldBe(address)
        );
    }

    [Test]
    public void ShouldNotReturnBusinessrepresentativeWithInvalidParameters()
    {
        var construct = () => new BusinessRepresentative(
            string.Empty,
            string.Empty,
            string.Empty,
            PhoneNumber.Create("+36201111111"),
            new DateOnly(2020, 1, 1),
            new Address("line1", "line2", "city", null, "2", "HU"),
            DateTime.UtcNow);

        Should.Throw<ArgumentException>(construct);
    }
}
