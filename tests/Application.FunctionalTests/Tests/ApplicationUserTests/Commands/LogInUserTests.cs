using FitPass.Application.ApplicationUsers.Commands;
using FitPass.Application.Common.Exceptions;
using FitPass.Domain.Constants;

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

        var ex = await Should.ThrowAsync<ValidationException>(action);

        ex.Errors.ShouldContainKey("Email");
        ex.Errors["Email"].ShouldContain("Email is required.");
        ex.Errors.ShouldContainKey("Password");
        ex.Errors["Password"].ShouldContain("Password is required.");
    }

    [Test]
    public async Task ShouldRequireValidEmail()
    {
        var command = new LogInUserCommand("invalid", "Password123_");

        var action = () => SendAsync(command);

        var ex = await action.ShouldThrowAsync<ValidationException>();

        ex.Errors.ShouldContainKey("Email");
        ex.Errors["Email"].ShouldContain("Valid email address is required");
    }

    [Test]
    public async Task ShouldRejectCredentialsExceedingMaxLimit()
    {
        var email = $"{new string('a', MaxStringLengths.Email + 1)}@localhost";
        var password = $"{new string('a', MaxStringLengths.Password)}Password123";

        var command = new LogInUserCommand(email, password);

        var action = () => SendAsync(command);

        var ex = await action.ShouldThrowAsync<ValidationException>();

        ex.Errors.ShouldContainKey("Email");
        ex.Errors["Email"].ShouldContain($"Email cannot be longer than {MaxStringLengths.Email} characters.");
        ex.Errors.ShouldContainKey("Password");
        ex.Errors["Password"].ShouldContain($"Password cannot be longer than {MaxStringLengths.Password} characters.");
    }

    [Test]
    public async Task ShouldThrowForEmailThatDoesNotBelongToAnyUser()
    {
        var command = new LogInUserCommand("emailThatDoesNotExist@localhost", "Password123_");

        var action = () => SendAsync(command);

        var ex = await action.ShouldThrowAsync<UnauthorizedAccessException>();
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
