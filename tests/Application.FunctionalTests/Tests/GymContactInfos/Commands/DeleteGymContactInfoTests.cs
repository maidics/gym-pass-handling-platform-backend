using FitPass.Application.FunctionalTests.Common.Extensions;

using FitPass.Application.FunctionalTests.TestData;
using FitPass.Application.GymContactInfos.Commands;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;

namespace FitPass.Application.FunctionalTests.Tests.GymContactInfos.Commands;

using static Testing;

public class DeleteGymContactInfoTests : BaseTestFixture
{
    [Test]
    public override void AuthorizeAttributeCheck()
    {
        ShouldRequireAuthorization<DeleteGymContactInfoCommand>(Roles.GymAdministrator);
    }

    [Test]
    public async Task ShouldThrowIfParametersAreInvalid()
    {
        await RunAsGymEmployeeAsync(Roles.GymAdministrator);

        var command = new DeleteGymContactInfoCommand(string.Empty);

        await ShouldThrowIfParametersAreInvalidAsync(command);
    }

    [Test]
    public async Task ShouldDeleteGymContactInfo()
    {
        var contact = new GymContactInfo()
        {
            Address = null,
            Email = null,
            PhoneNumber = null,
            FullName = "Full Name",
        };

        var obj = await TestEntityBuilder.BuildGymEmployeeAsync(
            Roles.GymAdministrator,
            gymContactInfos: [contact]
        );

        await RunAsUserAsync(obj.user);

        var command = new DeleteGymContactInfoCommand(contact.Id);

        var result = await SendAsync(command);
        result.ShouldBeSuccessful();

        var deletedContact = await FindAsync<GymContactInfo>(contact.Id);
        deletedContact.ShouldBeNull();
    }
}
