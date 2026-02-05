using FitPass.Application.Common.Exceptions;
using FitPass.Application.Common.Models;
using FitPass.Application.FunctionalTests.Common.Extensions;
using FitPass.Application.FunctionalTests.Infrastructure.Testing;
using FitPass.Application.Users.Commands.Emails;

namespace FitPass.Application.FunctionalTests.Tests.UserTests.Commands.Emails;

using static Testing;

public class SendPasswordResetEmailTests : BaseTestFixture
{
    [Test]
    public override void AuthorizeAttributeCheck()
    {
        ShouldNotRequireAuthorization<SendPasswordResetEmailCommand>();
    }

    [TestCase("")]
    [TestCase("invalid@email")]
    public async Task ShouldThrowIfParametersAreInvalid(string email)
    {
        var command = new SendPasswordResetEmailCommand(email);

        await ShouldThrowIfParametersAreInvalidAsync(command);
    }

    [Test]
    public async Task ShouldReturnSuccessIfUserNotExist()
    {
        var command = new SendPasswordResetEmailCommand("doesnotexists@test.com");

        var result = await SendAsync(command);
        result.ShouldBeSuccessful();
    }

    [Test]
    public async Task ShouldReturnSuccessIfUserExists()
    {
        var user = await CreateUserAsync();

        var command = new SendPasswordResetEmailCommand(user.Email!);

        var result = await SendAsync(command);
        result.ShouldBeSuccessful();

        EmailFolderShouldContainEmails();
    }
}
