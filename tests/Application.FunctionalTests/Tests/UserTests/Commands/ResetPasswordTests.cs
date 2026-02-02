using FitPass.Application.FunctionalTests.Infrastructure.Testing;

namespace FitPass.Application.FunctionalTests.Tests.UserTests.Commands;

using FitPass.Application.Common.Exceptions;
using FitPass.Application.Common.Models;
using FitPass.Application.Users.Commands;
using static Testing;

public class ResetPasswordTests : BaseTestFixture
{
    [Test]
    public override void AuthorizeAttributeCheck()
    {
        ShouldNotRequireAuthorization<ResetPasswordCommand>();
    }

    [Test]
    public async Task ShouldThrowIfParametersAreInvalid()
    {
        var command = new ResetPasswordCommand(string.Empty, string.Empty, string.Empty, "a");

        await Should.ThrowAsync<ValidationException>(SendAsync(command));
    }

    [Test]
    public async Task ShouldReturnNotFoundIfUserNotExists()
    {
        var command = new ResetPasswordCommand(
            Uri.EscapeDataString("notExists"),
            "token",
            "Password123_",
            "Password123_"
        );

        var result = await SendAsync(command);
        result.Type.ShouldBe(ResultTypes.NotFound);
        result.Message.ShouldNotBeEmpty();
    }

    [Test]
    public async Task ShouldResetPasswordForUserWithNoPassword()
    {
        var user = await CreateUserAsync(password: null);

        var token = await GeneratePasswordResetTokenAsync(user.Id);

        var command = new ResetPasswordCommand(
            Uri.EscapeDataString(user.Id),
            Uri.EscapeDataString(token),
            "Password123_",
            "Password123_"
        );

        await Should.NotThrowAsync(SendAsync(command));
    }

    [Test]
    public async Task ShouldResetPasswordForUserThatHasPassword()
    {
        var user = await CreateUserAsync();

        var token = await GeneratePasswordResetTokenAsync(user.Id);

        var command = new ResetPasswordCommand(
            Uri.EscapeDataString(user.Id),
            Uri.EscapeDataString(token),
            "Password123_",
            "Password123_"
        );

        await Should.NotThrowAsync(SendAsync(command));
    }
}
