using FitPass.Application.ApplicationUsers.Commands;
using FitPass.Application.Common.Exceptions;

namespace FitPass.Application.FunctionalTests.Tests.ApplicationUserTests.Commands;

using static Testing;

public class LogInUserTests : BaseTestFixture
{
    [Test]
    public async Task ShouldLogInUserWithValidCredentials()
    {
        var user = await ApplicationUserBuilder.BuildAsync();

        var tokenResponse = await SendAsync(new LogInUserCommand(user.Email!, "Password123_"));

        tokenResponse.ShouldNotBeNull();
        tokenResponse.AccessToken.ShouldNotBeNullOrEmpty();
    }

    [Test]
    public async Task ShouldThrowWithEmptyCredentials()
    {
        var command = new LogInUserCommand(string.Empty, string.Empty);

        var action = () => SendAsync(command);

        await Should.ThrowAsync<ValidationException>(action);
    }

    [Test]
    public async Task ShouldRequireValidEmail()
    {
        var command = new LogInUserCommand("invalid", "Password123_");

        var action = () => SendAsync(command);

        await action.ShouldThrowAsync<ValidationException>();
    }

    [Test]
    public async Task ShouldThrowForEmailThatDoesNotBelongToAnyUser()
    {
        var command = new LogInUserCommand("emailThatDoesNotExist@localhost", "Password123_");

        var action = () => SendAsync(command);

        await action.ShouldThrowAsync<UnauthorizedAccessException>();
    }

    [Test]
    public async Task ShouldBeCaseInsensitiveForEmail()
    {
        var user = await ApplicationUserBuilder.BuildAsync();

        var command = new LogInUserCommand(user.Email!.ToUpperInvariant(), "Password123_");

        var action = () => SendAsync(command);

        await action.ShouldNotThrowAsync();
    }

    public override void AuthorizeAttributeCheck()
    {
        throw new NotImplementedException();
    }
}
