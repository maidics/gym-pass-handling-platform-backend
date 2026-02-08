using FitPass.Application.Common.Exceptions;
using FitPass.Application.Common.Models;
using FitPass.Application.FunctionalTests.Common.Extensions;

using FitPass.Application.Users.Commands;

namespace FitPass.Application.FunctionalTests.Tests.UserTests.Commands;

using static Testing;

public class ResetPasswordTests : BaseTestFixture
{
    [Test]
    public override void AuthorizeAttributeCheck()
    {
        ShouldNotRequireAuthorization<ResetPasswordCommand>();
    }

    [TestCase("", "token", "Password123!", "Password123!")]
    [TestCase("id", "", "Password123!", "Password123!")]
    [TestCase("id", "token", "", "Password123!")]
    [TestCase("id", "token", "password", "Password123!")]
    [TestCase("id", "token", "Password123_", "Password123!")]
    [TestCase("id", "token", "Password123!", "")]
    public async Task ShouldThrowIfParametersAreInvalid(
        string encodedUserId,
        string encodedPasswordResetToken,
        string newPassword,
        string newPasswordConfirm
    )
    {
        var command = new ResetPasswordCommand(
            encodedUserId,
            encodedPasswordResetToken,
            newPassword,
            newPasswordConfirm
        );

        await ShouldThrowIfParametersAreInvalidAsync(command);
    }

    [Test]
    public async Task ShouldReturnNotFoundIfUserNotExists()
    {
        var command = new ResetPasswordCommand("id", "token", "Password123_", "Password123_");

        var result = await SendAsync(command);
        result.ShouldBeFailed(ResultTypes.NotFound);
    }

    [Test]
    public async Task ShouldResetPasswordForUserWithNoPassword()
    {
        var user = await CreateUserAsync(password: null);
        var token = await GeneratePasswordResetTokenAsync(user.Id);

        var command = new ResetPasswordCommand(
            Uri.EscapeDataString(user.Id),
            Uri.EscapeDataString(token),
            "Password123!",
            "Password123!"
        );

        var result = await SendAsync(command);
        result.ShouldBeSuccessful();
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

        var result = await SendAsync(command);
        result.ShouldBeSuccessful();
    }

    [Test]
    public async Task ShouldReturnFailureIfPasswordToResetToIsTheSameAsCurrentPassword()
    {
        var user = await CreateUserAsync();
        var token = await GeneratePasswordResetTokenAsync(user.Id);

        var command = new ResetPasswordCommand(
            Uri.EscapeDataString(user.Id),
            Uri.EscapeDataString(token),
            "Password123!",
            "Password123!"
        );

        var result = await SendAsync(command);
        result.ShouldBeFailed(ResultTypes.Conflict);
    }
}
