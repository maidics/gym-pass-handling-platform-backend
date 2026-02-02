using FitPass.Application.Common.Exceptions;
using FitPass.Application.Common.Models;
using FitPass.Application.FunctionalTests.Infrastructure.Testing;
using FitPass.Application.Users.Commands.Emails;

namespace FitPass.Application.FunctionalTests.Tests.UserTests.Commands.Emails;

using static Testing;

public class RequestPasswordResetEmailTests : BaseTestFixture
{
    [Test]
    public override void AuthorizeAttributeCheck()
    {
        ShouldNotRequireAuthorization<SendPasswordResetEmailCommand>();
    }

    [Test]
    public async Task ShouldThrowIfParametersAreInvalid()
    {
        var command = new SendPasswordResetEmailCommand("invalidEmail");

        await Should.ThrowAsync<ValidationException>(SendAsync(command));
    }

    [Test]
    public async Task ShouldReturnSuccessIfUserNotExist()
    {
        var command = new SendPasswordResetEmailCommand("userNotExist@localhost");

        var result = await SendAsync(command);
        result.Succeeded.ShouldBeTrue();
    }

    [Test]
    public async Task ShouldReturnSuccessIfUserExists()
    {
        var user = await CreateUserAsync();

        var command = new SendPasswordResetEmailCommand(user.Email!);

        var result = await SendAsync(command);
        result.Succeeded.ShouldBeTrue();
    }
}
