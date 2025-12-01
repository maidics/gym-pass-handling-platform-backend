using FitPass.Domain.ValueObjects;
using NUnit.Framework;
using Shouldly;

namespace FitPass.Domain.UnitTests.ValueObjects;

public class PhoneNumberTests
{
    [TestCase("+36201111111")]
    [TestCase("36201111111")]
    [TestCase("36 20 111 1111")]
    [TestCase("3620 1 11 1111")]
    public void ShouldReturnPhoneNumber(string phoneNumberString)
    {
        var phoneNumber = PhoneNumber.Create(phoneNumberString);

        phoneNumber.Value.ShouldBe("36201111111");
    }

    [TestCase("")]
    [TestCase("phonenumber")]
    [TestCase("-36202221111")]
    [TestCase("36202221111114325612")]
    [TestCase("+362022211111143256")]
    public void ShouldThrowIfParameterIsNotValid(string phoneNumberString)
    {
        Should.Throw<ArgumentException>(() => PhoneNumber.Create(phoneNumberString));
    }

    [TestCase("+36201111111")]
    [TestCase("36201111111")]
    [TestCase("36 20 111 1111")]
    [TestCase("3620 1 11 1111")]
    public void IsValidShouldReturnTrueForValidPhoneNumbers(string phoneNumberString)
    {
        PhoneNumber.IsValid(phoneNumberString).ShouldBeTrue();
    }

    [TestCase("")]
    [TestCase("phonenumber")]
    [TestCase("-36202221111")]
    [TestCase("36202221111114325612")]
    [TestCase("+362022211111143256")]
    public void IsValidShouldReturnFalseForInvalidPhoneNumbers(string phoneNumberString)
    {
        PhoneNumber.IsValid(phoneNumberString).ShouldBeFalse();
    }
}
