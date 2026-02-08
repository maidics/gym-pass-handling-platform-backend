using FitPass.Application.Common.Exceptions;
using FitPass.Application.Common.Models;
using FitPass.Application.FunctionalTests.Common.Extensions;

using FitPass.Application.Users.Commands;
using FitPass.Domain.Strings;

namespace FitPass.Application.FunctionalTests.Tests.UserTests.Commands;

using static Testing;

public class LogInUserTests : BaseTestFixture
{
    [Test]
    public override void AuthorizeAttributeCheck()
    {
        ShouldNotRequireAuthorization<LogInUserCommand>();
    }

    [TestCase("", "Password123!")]
    [TestCase("invalid@email", "Password123!")]
    [TestCase("email@test.com", "")]
    public async Task ShouldThrowIfParametersAreInvalid(string email, string password)
    {
        var command = new LogInUserCommand(email, password);

        await ShouldThrowIfParametersAreInvalidAsync(command);
    }

    [Test]
    public async Task ShouldReturnUnauthorizedIfTheEmailIsNotInUse()
    {
        var command = new LogInUserCommand("email@test.com", "password");

        var result = await SendAsync(command);
        result.ShouldBeFailed(ResultTypes.Unauthorized);
    }

    [Test]
    public async Task ShouldLogInUserWithValidCredentials()
    {
        var user = await CreateUserAsync();

        var result = await SendAsync(new LogInUserCommand(user.Email!, "Password123!"));
        result.ShouldBeSuccessful();

        var jwt = result.Value;
        jwt.AccessToken.ShouldNotBeNull();
    }

    [Test]
    public async Task ShouldBeCaseInsensitiveForEmail()
    {
        var user = await CreateUserAsync();

        var command = new LogInUserCommand(user.Email!.ToUpperInvariant(), "Password123!");

        var result = await SendAsync(command);
        result.ShouldBeSuccessful();

        var jwt = result.Value;
        jwt.AccessToken.ShouldNotBeNull();
    }

    [Test]
    public async Task ShouldReturnUnauthorizedIfUserAccountIsNotActivated()
    {
        var user = await CreateUserAsync(password: null, emailConfirmed: false);

        var command = new LogInUserCommand(user.Email!, "Password123_");

        var result = await SendAsync(command);
        result.ShouldBeFailed(ResultTypes.Unauthorized);
    }

    [Test]
    public async Task ShouldReturnUnauthorizedIfPasswordIsIncorrect()
    {
        var user = await CreateUserAsync();

        var command = new LogInUserCommand(user.Email!, "Password123");

        var result = await SendAsync(command);
        result.ShouldBeFailed(ResultTypes.Unauthorized);
    }
}
