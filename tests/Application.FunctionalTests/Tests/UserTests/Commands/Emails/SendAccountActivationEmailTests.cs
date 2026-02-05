using FitPass.Application.Common.Models;
using FitPass.Application.FunctionalTests.Common.Extensions;
using FitPass.Application.FunctionalTests.Infrastructure.Testing;
using FitPass.Application.FunctionalTests.TestData;
using FitPass.Application.Users.Commands.Emails;

namespace FitPass.Application.FunctionalTests.Tests.UserTests.Commands.Emails;

using static Testing;

public class SendAccountActivationEmailTests : BaseTestFixture
{
    [Test]
    public override void AuthorizeAttributeCheck()
    {
        ShouldNotRequireAuthorization<SendAccountActivationEmailCommand>();
    }

    [TestCase("")]
    [TestCase("invalid@email")]
    public async Task ShouldThrowIfParametersAreInvalid(string email)
    {
        var command = new SendAccountActivationEmailCommand(email);

        await ShouldThrowIfParametersAreInvalidAsync(command);
    }

    [Test]
    public async Task ShouldReturnSuccessIfUserIsNotFound()
    {
        var command = new SendAccountActivationEmailCommand("user@test.com");

        var result = await SendAsync(command);
        result.ShouldBeSuccessful();
    }

    [Test]
    public async Task ShouldReturnBusinessRuleViolationIfUserEmailIsConfirmed()
    {
        var obj = await TestEntityBuilder.BuildDefaultUserAsync(true);

        await RunAsUserAsync(obj.user);

        var command = new SendAccountActivationEmailCommand(obj.user.Email!);

        var result = await SendAsync(command);
        result.ShouldBeFailed(ResultTypes.BusinessRuleViolation);
    }

    [Test]
    public async Task ShouldReturnBusinessRuleViolationIfUserAlreadyHasPassword()
    {
        var obj = await RunAsDefaultUserAsync();

        var command = new SendAccountActivationEmailCommand(obj.user.Email!);

        var result = await SendAsync(command);
        result.ShouldBeFailed(ResultTypes.BusinessRuleViolation);
    }

    [Test]
    public async Task ShouldSendAccountActivationEmail()
    {
        var obj = await RunAsDefaultUserAsync();

        var command = new SendAccountActivationEmailCommand(obj.user.Email!);

        var result = await SendAsync(command);
        result.ShouldBeSuccessful();

        EmailFolderShouldContainEmails();
    }
}
