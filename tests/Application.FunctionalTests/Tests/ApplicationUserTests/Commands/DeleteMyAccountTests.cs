using FitPass.Application.ApplicationUsers.Commands;

namespace FitPass.Application.FunctionalTests.Tests.ApplicationUserTests.Commands;

using static Testing;

public class DeleteMyAccountTests : BaseTestFixture
{
    [Test]
    public async Task ShouldDenyAnonymousUser()
    {
        var command = new DeleteMyAccountCommand();

        var action = () => SendAsync(command);

        await Should.ThrowAsync<UnauthorizedAccessException>(action);
    }

    [Test]
    public async Task ShouldThrowIfUserDoesNotExist()
    {
        var command = new DeleteMyAccountCommand();

        SetLoggedInUserId("invalidUserId");

        var action = () => SendAsync(command);

        await action.ShouldThrowAsync<Exception>();
    }

    [Test]
    public async Task ShouldDeleteUserAccount()
    {
        var user = await RunAsDefaultUserAsync();

        var userProfile = await UserProfileBuilder.WithApplicationUserId(user.Id).BuildAsync();

        var command = new DeleteMyAccountCommand();

        var action = () => SendAsync(command);

        await action.ShouldNotThrowAsync();
    }

    [Test]
    public override void AuthorizeAttributeCheck()
    {
        var command = new DeleteMyAccountCommand();

        var hasAuthorizeAttirbute = HasAuthorizeAttribute<DeleteMyAccountCommand>();

        hasAuthorizeAttirbute.ShouldBeTrue();
    }
}
