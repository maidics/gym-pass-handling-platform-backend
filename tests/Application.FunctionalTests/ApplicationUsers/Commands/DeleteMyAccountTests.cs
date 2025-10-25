using FitPass.Application.ApplicationUsers.Commands;
using FitPass.Application.Common.Security;
using FitPass.Domain.Entities;

namespace FitPass.Application.FunctionalTests.ApplicationUsers.Commands;

using static Testing;

public class DeleteMyAccountTests : BaseTestFixture
{
    [Test]
    public async Task ShouldDenyAnonymousUser()
    {
        var command = new DeleteMyAccountCommand();

        var action = () => SendAsync(command);

        command.GetType().ShouldSatisfyAllConditions(type => type.ShouldBeDecoratedWith<AuthorizeAttribute>());

        await Should.ThrowAsync<UnauthorizedAccessException>(action);
    }

    [Test]
    public async Task ShouldThrowIfUserDoesNotExist()
    {
        var command = new DeleteMyAccountCommand();

        SetLoggedInUserId("invalidUserId");

        var action = () => SendAsync(command);

        await action.ShouldThrowAsync<UnauthorizedAccessException>();
    }

    [Test]
    public async Task ShouldDeleteUserAccount()
    {
        await RunAsDefaultUserAsync();

        var command = new DeleteMyAccountCommand();

        var action = () => SendAsync(command);

        await action.ShouldNotThrowAsync();
    }

    [Test]
    public async Task ShouldDeleteAllUserAccounts()
    {
        await RunAsDefaultUserAsync();

        await SendAsync(new DeleteMyAccountCommand());

        await RunAsAppAdminAsync();

        await SendAsync(new DeleteMyAccountCommand());

        await RunAsGymAdminAsync();

        await SendAsync(new DeleteMyAccountCommand());

        await RunAsGymStaffAsync();

        await SendAsync(new DeleteMyAccountCommand());

        await RunAsPendingGymManagementAsync();

        await SendAsync(new DeleteMyAccountCommand());

        var count = await CountAsync<ApplicationUser>();

        count.ShouldBe(0);
    }
}
