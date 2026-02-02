using FitPass.Application.FunctionalTests.Infrastructure.Testing;
using FitPass.Application.FunctionalTests.TestData;
using FitPass.Application.GymContactInfos.Commands;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using FitPass.Domain.ValueObjects;

namespace FitPass.Application.FunctionalTests.Tests.GymContactInfos.Commands;

using static Testing;

public class UpdateGymContactInfoTests : BaseTestFixture
{
    [Test]
    public override void AuthorizeAttributeCheck()
    {
        ShouldRequireAuthorization<UpdateGymContactInfoCommand>(Roles.GymAdministrator);
    }

    [TestCase("", "", "", "")]
    [TestCase("id", null, null, "Full Name")]
    [TestCase("id", "invalid@email", "invalidPhone", "Full Name")]
    [TestCase("id", "valid@email.com", "1234567890123456789", "Full Name")]
    public async Task ShouldThrowIfParametersAreInvalid(
        string contactInfoId,
        string? email,
        string? phoneNumber,
        string fullName
    )
    {
        await RunAsGymEmployeeAsync(Roles.GymAdministrator);

        var command = new UpdateGymContactInfoCommand(
            contactInfoId,
            email,
            phoneNumber,
            fullName,
            null
        );

        await ShouldThrowIfParametersAreInvalidAsync(command);
    }

    [TestCase("valid@email.com", "+36201111111", "Full Name", true)]
    [TestCase(null, "+36201111111", "Full Name", false)]
    [TestCase(null, "+36201111111", "Full Name", true)]
    [TestCase("valid@email.com", null, "Full Name", false)]
    [TestCase("valid@email.com", null, "Full Name", true)]
    public async Task ShouldUpdateGymContactInfo(
        string? email,
        string? phoneNumber,
        string fullName,
        bool useAddress
    )
    {
        var contact = new GymContactInfo
        {
            Address = null,
            Email = "gym@info.com",
            PhoneNumber = PhoneNumber.Create("+36201111111"),
            FullName = "Full Name",
        };

        var obj = await TestEntityBuilder.BuildGymEmployeeAsync(
            Roles.GymAdministrator,
            gymContactInfos: [contact]
        );

        await RunAsUserAsync(obj.user);

        var address = useAddress
            ? new Address("Test street 1", null, "TestCity", null, "1111", "HU")
            : null;

        var command = new UpdateGymContactInfoCommand(
            contact.Id,
            email,
            phoneNumber,
            fullName,
            address
        );

        var result = await SendAsync(command);
        result.ShouldBeSuccessful();

        var updatedContact = await FindAsync<GymContactInfo>(contact.Id);
        updatedContact.ShouldNotBeNull();
        updatedContact.Address.ShouldBe(address);
        updatedContact.FullName.ShouldBe(fullName);
        updatedContact.Email.ShouldBe(email);
        updatedContact.PhoneNumber?.Value.ShouldBe(phoneNumber);
    }
}
