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
        var obj = await RunAsDefaultUserAsync();

        var command = new DeleteMyAccountCommand();

        var action = () => SendAsync(command);

        await action.ShouldNotThrowAsync();

        var deletedUserProfile = await FindAsync<UserProfile>(obj.userProfile.ApplicationUserId);
        deletedUserProfile.ShouldBeNull();
    }

    [Test]
    public async Task ShouldNotDeleteAppAdminAccount()
    {
        await RunAsAppAdminAsync();

        var command = new DeleteMyAccountCommand();

        await Should.ThrowAsync<UnauthorizedAccessException>(SendAsync(command));
    }

    [Test]
    public override void AuthorizeAttributeCheck()
    {
        ShouldRequireAuthorization<DeleteMyAccountCommand>();
    }
}
