using FitPass.Domain.ValueObjects;
using NUnit.Framework;
using Shouldly;

namespace FitPass.Domain.UnitTests.ValueObjects;

public class BusinessRepresentativeTests
{
    private static readonly PhoneNumber _phoneNumber = PhoneNumber.Create("+36201111111"); //by the time it gets to the constructor this will be valid anyways
    private static readonly Address _address = new Address("Fehér utca 2", "B/34", "Veszprém", "Győr-Moson-Sopron megye", "8200", "HU");

    private static IEnumerable<object[]> ValidTestData()
    {
        yield return new object[]
        {
            "Lajos",
            "Kovács",
            "kovacs.lajos@localhost",
            _phoneNumber,
            new DateOnly(1990, 10, 1),
            _address,
            DateTimeOffset.UtcNow
        };
    }

    [TestCaseSource(nameof(ValidTestData))]
    public void ShouldReturnBusinessRepresentative(string firstName, string lastName, string email, PhoneNumber phoneNumber, DateOnly dateOfBirth, Address address, DateTimeOffset utcNow)
    {
        var br = new BusinessRepresentative(
            firstName,
            lastName,
            email,
            phoneNumber,
            dateOfBirth, 
            address,
            utcNow);

        br.ShouldSatisfyAllConditions(
            () => br.FirstName.ShouldBe(firstName),
            () => br.LastName.ShouldBe(lastName),
            () => br.Email.ShouldBe(email),
            () => br.Phone.ShouldBe(phoneNumber),
            () => br.DateOfBirth.ShouldBe(dateOfBirth),
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
