namespace FitPass.Application.FunctionalTests.Tests.ApplicationUserTests.Commands;

using Fitpass.Application.Common.Exceptions;
using FitPass.Application.ApplicationUsers.Commands;
using FitPass.Application.ApplicationUsers.DTOs;
using FitPass.Application.Common.Exceptions;
using FitPass.Domain.Strings;
using FitPass.Infrastructure.Identity;
using static Testing;

public class ActivateUserAccountTests : BaseTestFixture
{
    [Test]
    public override void AuthorizeAttributeCheck()
    {
        var hasAuthorizeAttribute = HasAuthorizeAttribute<ActivateUserAccountCommand>();

        hasAuthorizeAttribute.ShouldBeFalse();
    }

    [Test]
    public async Task ShouldDenyInvalidParameters()
    {
        var command = new ActivateUserAccountCommand(string.Empty, string.Empty, null, null);

        await Should.ThrowAsync<ValidationException>(() => SendAsync(command));
    }

    [Test]
    public async Task ShouldThrowIfUserNotExists()
    {
        var command = new ActivateUserAccountCommand(Uri.EscapeDataString("email@localhost"), "token", null, null);

        await Should.ThrowAsync<NotFoundException>(() => SendAsync(command));
    }

    [Test]
    public async Task ShouldThrowIfEmailConfirmationTokenDoesNotExist()
    {
        var user = await ApplicationUserBuilder.BuildAsync();

        var command = new ActivateUserAccountCommand(Uri.EscapeDataString(user.Email!), "invalidtoken", null, null);

        await Should.ThrowAsync<Exception>(() => SendAsync(command));
    }

    [Test]
    public async Task ShouldConfirmUserEmail()
    {
        var user = await ApplicationUserBuilder.BuildAsync();

        var token = await GenerateEmailConfirmationToken(user.Id);

        var command = new ActivateUserAccountCommand(Uri.EscapeDataString(user.Email!), token, null, null);

        var jwtToken = await SendAsync(command);

        jwtToken.ShouldBeOfType<JwtToken>();

        var updateUser = await FindAsync<ApplicationUser>(user.Id);

        updateUser!.EmailConfirmed.ShouldBeTrue();
    }

    [Test]
    public async Task ShouldNotAddPasswordToUserThatHasPassword()
    {
        var user = await ApplicationUserBuilder.WithPassword("Password123_").BuildAsync();

        var token = await GenerateEmailConfirmationToken(user.Id);

        var command = new ActivateUserAccountCommand(Uri.EscapeDataString(user.Email!), token, "Password123_", "Password123_");

        var ex = await Should.ThrowAsync<BadRequestException>(() => SendAsync(command));

        ex.Message.ShouldContain(ResultErrorMessages.UserAlreadyHasPassword());
    }

    [Test]
    public async Task ShouldAddPasswordToUserThatHasNoPassword()
    {
        var user = await ApplicationUserBuilder.BuildAsync();

        var token = await GenerateEmailConfirmationToken(user.Id);

        var command = new ActivateUserAccountCommand(Uri.EscapeDataString(user.Email!), token, "Password123_", "Password123_");

        var jwtToken = await SendAsync(command);

        jwtToken.ShouldBeOfType<JwtToken>();

        var updateUser = await FindAsync<ApplicationUser>(user.Id);

        updateUser!.EmailConfirmed.ShouldBeTrue();
        updateUser!.PasswordHash.ShouldNotBeNull();
    }
}
