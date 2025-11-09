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

        await Should.ThrowAsync<ValidationException>(SendAsync(command));
    }

    [Test]
    public async Task ShouldUpdateUserProfile()
    {
        var user = await RunAsDefaultUserAsync();

        var userProfile = await UserProfileBuilder
            .WithApplicationUserId(user.Id)
            .WithFirstName("First")
            .WithLastName("Last")
            .BuildAsync();

        var newFirstName = "FirstNew";
        var newLastName = "LastNew";

        var command = new UpdateMyUserProfileCommand(newFirstName, newLastName);

        await Should.NotThrowAsync(SendAsync(command));

        var updateUserProfile = await FindAsync<UserProfile>(userProfile.ApplicationUserId);

        updateUserProfile.ShouldNotBeNull();
        updateUserProfile.FirstName.ShouldBe(newFirstName);
        updateUserProfile.LastName.ShouldBe(newLastName);
    }
}
