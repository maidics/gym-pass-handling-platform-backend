using FitPass.Application.Common.Exceptions;
using FitPass.Application.Common.Models;
using FitPass.Application.FunctionalTests.Infrastructure.Testing;
using FitPass.Application.Users.Commands;
using FitPass.Domain.Strings;

namespace FitPass.Application.FunctionalTests.Tests.UserTests.Commands;

using static Testing;

public class LogInUserTests : BaseTestFixture
{
    [Test]
    public async Task ShouldLogInUserWithValidCredentials()
    {
        var user = await CreateUserAsync();

        var result = await SendAsync(new LogInUserCommand(user.Email!, "Password123!"));
        result.Succeeded.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
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
    public async Task ShouldReturnUnathorizedIfTheEmailIsNotInUse()
    {
        var command = new LogInUserCommand("email@test", "password");

        var result = await SendAsync(command);
        result.Type.ShouldBe(ResultTypes.Unauthorized);
    }

    [Test]
    public async Task ShouldBeCaseInsensitiveForEmail()
    {
        var user = await CreateUserAsync();

        var command = new LogInUserCommand(user.Email!.ToUpperInvariant(), "Password123!");

        await SendAsync(command);
    }

    [Test]
    public async Task ShoudlReturnUnauthorizedIfUserAccountIsNotActivated()
    {
        var user = await CreateUserAsync(password: null, emailConfirmed: false);

        var command = new LogInUserCommand(user.Email!, "Password123_");

        var result = await SendAsync(command);
        result.Type.ShouldBe(ResultTypes.Unauthorized);
        result.Message.ShouldNotBeEmpty();
    }

    [Test]
    public override void AuthorizeAttributeCheck()
    {
        ShouldNotRequireAuthorization<LogInUserCommand>();
    }
}
