using FitPass.Application.FunctionalTests.Infrastructure.Testing;

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

        await Should.ThrowAsync<ValidationException>(() => SendAsync(command));
    }

    [Test]
    public async Task ShouldReturnNotFoundIfUserNotFound()
    {
        var command = new ActivateUserAccountCommand(
            Uri.EscapeDataString("email@localhost"),
            "token",
            false,
            null,
            null
        );

        var result = await SendAsync(command);
        result.Type.ShouldBe(ResultTypes.NotFound);
        result.Message.ShouldNotBeEmpty();
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
        result.Type.ShouldBe(ResultTypes.BusinessRuleViolation);
        result.Message.ShouldNotBeEmpty();
    }

    [Test]
    public async Task ShoudlReturnForbiddenIfTokenIsNotValid()
    {
        var user = await CreateUserAsync(password: null);

        var command = new ActivateUserAccountCommand(
            Uri.EscapeDataString(user.Email!),
            "invalidtoken",
            true,
            "Password123!",
            "Password123!"
        );

        var result = await SendAsync(command);
        result.Type.ShouldBe(ResultTypes.Forbidden);
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
        result.Type.ShouldBe(ResultTypes.Forbidden);
        result.Message.ShouldNotBeEmpty();
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
        result.Succeeded.ShouldBeTrue();
        result.Value.ShouldNotBeNull();

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
        result.Succeeded.ShouldBeTrue();
        result.Value.ShouldNotBeNull();

        var updatedUser = await FindAsync<ApplicationUser>(user.Id);

        updatedUser!.EmailConfirmed.ShouldBeTrue();
        updatedUser!.PasswordHash.ShouldNotBeNull();
    }
}
