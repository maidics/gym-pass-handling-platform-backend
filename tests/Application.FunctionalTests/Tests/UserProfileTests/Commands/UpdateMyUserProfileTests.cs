using FitPass.Application.Common.Exceptions;
using FitPass.Application.UserProfiles.Commands;
using FitPass.Domain.Entities;

namespace FitPass.Application.FunctionalTests.Tests.UserProfileTests.Commands;

using static Testing;

public class UpdateMyUserProfileTests : BaseTestFixture
{
    [Test]
    public override void AuthorizeAttributeCheck()
    {
        ShouldRequireAuthorization<UpdateMyUserProfileCommand>();
    }

    [Test]
    public async Task ShouldDenyInvalidParameters()
    {
        await RunAsDefaultUserAsync();

        var command = new UpdateMyUserProfileCommand(string.Empty, string.Empty);

        await ShouldThrowIfParametersAreInvalidAsync(command);
    }

    [Test]
    public async Task ShouldUpdateUserProfile()
    {
        var obj = await RunAsDefaultUserAsync();

        var newFirstName = "FirstNew";
        var newLastName = "LastNew";

        var command = new UpdateMyUserProfileCommand(newFirstName, newLastName);

        await SendAsync(command);

        var updatedUserProfile = await FindAsync<UserProfile>(obj.userProfile.Id);

        updatedUserProfile.ShouldNotBeNull();
        updatedUserProfile.FirstName.ShouldBe(newFirstName);
        updatedUserProfile.LastName.ShouldBe(newLastName);
    }
}
