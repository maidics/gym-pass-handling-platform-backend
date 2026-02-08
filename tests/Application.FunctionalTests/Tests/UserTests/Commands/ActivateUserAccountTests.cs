using FitPass.Application.FunctionalTests.Common.Extensions;


namespace FitPass.Application.FunctionalTests.Tests.UserTests.Commands;

using FitPass.Application.Common.Exceptions;
using FitPass.Application.Common.Models;
using FitPass.Application.Users.Commands;
using FitPass.Application.Users.DTOs;
using FitPass.Infrastructure.Identity;
using static Testing;

public class ActivateUserAccountTests : BaseTestFixture
{
    [Test]
    public override void AuthorizeAttributeCheck()
    {
        ShouldNotRequireAuthorization<ActivateUserAccountCommand>();
    }

    [Test]
    public async Task ShouldThrowIfParametersAreInvalid()
    {
        var command = new ActivateUserAccountCommand(string.Empty, string.Empty, true, null, null);

        await ShouldThrowIfParametersAreInvalidAsync(command);
    }

    [Test]
    public async Task ShouldReturnNotFoundIfUserNotFound()
    {
        var command = new ActivateUserAccountCommand(
            Uri.EscapeDataString("email@test.com"),
            "token",
            false,
            null,
            null
        );

        var result = await SendAsync(command);
        result.ShouldBeFailed(ResultTypes.NotFound);
    }

    [Test]
    public async Task ShouldReturnBusinessRuleViolationIfUserHasNoPasswordAndSetPasswordIsFalse()
    {
        var user = await CreateUserAsync(password: null);

        var token = await GenerateEmailConfirmationTokenAsync(user.Id);

        var command = new ActivateUserAccountCommand(
            Uri.EscapeDataString(user.Email!),
            Uri.EscapeDataString(token),
            false,
            null,
            null
        );

        var result = await SendAsync(command);
        result.ShouldBeFailed(ResultTypes.BusinessRuleViolation);
    }

    [Test]
    public async Task ShouldReturnForbiddenIfTokenIsNotValid()
    {
        var user = await CreateUserAsync(password: null);

        var command = new ActivateUserAccountCommand(
            Uri.EscapeDataString(user.Email!),
            "token",
            true,
            "Password123!",
            "Password123!"
        );

        var result = await SendAsync(command);
        result.ShouldBeFailed(ResultTypes.Forbidden);
    }

    [Test]
    public async Task ShouldReturnForbiddenIfUserAlreadyHasPasswordAndSetPasswordIsTrue()
    {
        var user = await CreateUserAsync();

        var token = await GenerateEmailConfirmationTokenAsync(user.Id);

        var command = new ActivateUserAccountCommand(
            Uri.EscapeDataString(user.Email!),
            Uri.EscapeDataString(token),
            true,
            "Password123_",
            "Password123_"
        );

        var result = await SendAsync(command);
        result.ShouldBeFailed(ResultTypes.Forbidden);
    }

    [Test]
    public async Task ShouldConfirmUserEmail()
    {
        var user = await CreateUserAsync();

        var token = await GenerateEmailConfirmationTokenAsync(user.Id);

        var command = new ActivateUserAccountCommand(
            Uri.EscapeDataString(user.Email!),
            token,
            false,
            null,
            null
        );

        var result = await SendAsync(command);
        result.ShouldBeSuccessful();

        var jwt = result.Value;
        jwt.ShouldNotBeNull();
        jwt.AccessToken.ShouldNotBeNull();

        var updatedUser = await FindAsync<ApplicationUser>(user.Id);
        updatedUser.ShouldNotBeNull();
        updatedUser.EmailConfirmed.ShouldBeTrue();
    }

    [Test]
    public async Task ShouldAddPasswordToUserThatHasNoPassword()
    {
        var user = await CreateUserAsync(password: null);

        var token = await GenerateEmailConfirmationTokenAsync(user.Id);

        var command = new ActivateUserAccountCommand(
            Uri.EscapeDataString(user.Email!),
            Uri.EscapeDataString(token),
            true,
            "Password123_",
            "Password123_"
        );

        var result = await SendAsync(command);
        result.ShouldBeSuccessful();

        var jwt = result.Value;
        jwt.ShouldNotBeNull();
        jwt.AccessToken.ShouldNotBeNull();

        var updatedUser = await FindAsync<ApplicationUser>(user.Id);
        updatedUser!.EmailConfirmed.ShouldBeTrue();
        updatedUser!.PasswordHash.ShouldNotBeNull();
    }
}
