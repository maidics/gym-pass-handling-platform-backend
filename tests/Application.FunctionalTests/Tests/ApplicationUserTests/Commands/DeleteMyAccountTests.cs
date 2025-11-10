using FitPass.Application.ApplicationUsers.Commands;
using FitPass.Domain.Entities;

namespace FitPass.Application.FunctionalTests.Tests.ApplicationUserTests.Commands;

using static Testing;

public class DeleteMyAccountTests : BaseTestFixture
{
    [Test]
    public async Task ShouldThrowIfUserDoesNotExist()
    {
        var command = new DeleteMyAccountCommand();

        SetLoggedInUserId("invalidUserId");

        var action = () => SendAsync(command);

        await action.ShouldThrowAsync<Exception>();
    }

    [Test]
    public async Task ShouldDeleteDefaultUserAccount()
    {
        var userObj = await RunAsDefaultUserAsync();

        var userProfile = await UserProfileBuilder.WithApplicationUserId(userObj.user.Id).BuildAsync();

        var command = new DeleteMyAccountCommand();

        var action = () => SendAsync(command);

        await action.ShouldNotThrowAsync();

        var deletedUserProfile = await FindAsync<UserProfile>(userObj.userProfile.ApplicationUserId);
        deletedUserProfile.ShouldBeNull();
    }

    [Test]
    public override void AuthorizeAttributeCheck()
    {
        ShouldRequireAuthorization<DeleteMyAccountCommand>();
    }
}
