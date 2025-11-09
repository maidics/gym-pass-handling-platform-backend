namespace FitPass.Application.FunctionalTests.Tests.ApplicationUserTests.Commands;

using FitPass.Application.ApplicationUsers.Commands;
using FitPass.Application.Common.Exceptions;
using static Testing;
public class ResetPasswordTests : BaseTestFixture
{
    [Test]
    public override void AuthorizeAttributeCheck()
    {
        var hasAuthorizeAttribute = HasAuthorizeAttribute<ResetPasswordCommand>();

        hasAuthorizeAttribute.ShouldBeFalse();
    }

    [Test]
    public async Task ShouldDenyInvalidParameters()
    {
        var command = new ResetPasswordCommand(string.Empty, string.Empty, string.Empty, "a");

        await Should.ThrowAsync<ValidationException>(SendAsync(command));
    }

    [Test]
    public async Task ShouldThrowIfUserNotExists()
    {
        var command = new ResetPasswordCommand(Uri.EscapeDataString("notExists"), "token", "Password123_", "Password123_");

        await Should.ThrowAsync<NotFoundException>(SendAsync(command));
    }

    [Test]
    public async Task ShouldResetPasswordForUserWithNoPassword()
    {
        var user = await ApplicationUserBuilder.BuildAsync();

        var token = await GeneratePasswordResetTokenAsync(user.Id);

        var command = new ResetPasswordCommand(Uri.EscapeDataString(user.Id), Uri.EscapeDataString(token), "Password123_", "Password123_");

        await Should.NotThrowAsync(SendAsync(command));
    }

    [Test]
    public async Task ShouldResetPasswordForUserThatHasPassword()
    {
        var user = await ApplicationUserBuilder.WithPassword("Password1234_").BuildAsync();

        var token = await GeneratePasswordResetTokenAsync(user.Id);

        var command = new ResetPasswordCommand(Uri.EscapeDataString(user.Id), Uri.EscapeDataString(token), "Password123_", "Password123_");

        await Should.NotThrowAsync(SendAsync(command));
    }
}
