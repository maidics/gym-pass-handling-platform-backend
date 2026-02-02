using FitPass.Application.FunctionalTests.Infrastructure.Testing;
using FitPass.Application.GymContactInfos.Commands;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using FitPass.Domain.ValueObjects;

namespace FitPass.Application.FunctionalTests.Tests.GymContactInfos.Commands;

using static Testing;

public class CreateGymContactInfoTests : BaseTestFixture
{
    [Test]
    public override void AuthorizeAttributeCheck()
    {
        ShouldRequireAuthorization<CreateGymContactInfoCommand>(Roles.GymAdministrator);
    }

    [TestCase("", "", "")]
    [TestCase("invalidPhone", "", "")]
    [TestCase(null, "invalidEmail", "")]
    [TestCase(null, null, "Name")]
    public async Task ShouldThrowIfParametersAreInvalid(
        string? phoneNumber,
        string? email,
        string fullName
    )
    {
        await RunAsGymEmployeeAsync(Roles.GymAdministrator);

        var command = new CreateGymContactInfoCommand(phoneNumber, email, fullName, null);

        await ShouldThrowIfParametersAreInvalidAsync(command);
    }

    [TestCase("+36201111111", "test@localhost.com", "Full Name", true)]
    [TestCase("+36201111111", "test@localhost.com", "Full Name", false)]
    [TestCase(null, "test@localhost.com", "Full Name", true)]
    [TestCase(null, "test@localhost.com", "Full Name", false)]
    [TestCase("+36201111111", null, "Full Name", true)]
    [TestCase("+36201111111", null, "Full Name", false)]
    public async Task ShouldCreateGymContactInfo(
        string? phoneNumber,
        string? email,
        string fullName,
        bool useAddress
    )
    {
        var obj = await RunAsGymEmployeeAsync(Roles.GymAdministrator);

        var address = useAddress
            ? new Address("Test street 2", null, "TestCity", null, "1111", "HU")
            : null;

        var command = new CreateGymContactInfoCommand(phoneNumber, email, fullName, address);

        var result = await SendAsync(command);

        result.ShouldBeSuccessful();

        var contact = await GetFirstAsync<GymContactInfo>();
        contact.ShouldNotBeNull();
        contact.FullName.ShouldBe(fullName);
        contact.Email.ShouldBe(email);
        contact.PhoneNumber?.Value.ShouldBe(phoneNumber);
        contact.Address.ShouldBe(address);
    }
}
