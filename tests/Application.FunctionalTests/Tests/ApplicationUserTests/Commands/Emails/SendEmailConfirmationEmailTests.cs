using FitPass.Application.ApplicationUsers.Commands.Emails;
using FitPass.Application.Common.Exceptions;

namespace FitPass.Application.FunctionalTests.Tests.ApplicationUserTests.Commands.Emails;

using static Testing;

public class SendEmailConfirmationEmailTests : BaseTestFixture
{
    [Test]
    public async Task ShouldDenyAnonymousUser()
    {
        var command = new SendEmailConfirmationEmailCommand("email@localhost");

        var action = () => SendAsync(command);

        action.ShouldThrow<UnauthorizedAccessException>();
    }

    [Test]
    public async Task ShouldDenyInvalidEmail()
    {
        await RunAsDefaultUserAsync();

        var command = new SendEmailConfirmationEmailCommand("invalidEmail");

        var action = () => SendAsync(command);

        action.ShouldThrow<ValidationException>();
    }

    [Test]
    public async Task ShouldThrowIfEmailIsNotAttachedToAnyUser()
    {
        await RunAsDefaultUserAsync();

        var command = new SendEmailConfirmationEmailCommand("not-user@email");

        var action = () => SendAsync(command);

        await action.ShouldThrowAsync<NotFoundException>();
    }

    [Test]
    public async Task ShouldReturnSuccess()
    {
        var user = await RunAsDefaultUserAsync();

        var command = new SendEmailConfirmationEmailCommand(user.Email!);

        var result = await SendAsync(command);

        result.Succeeded.ShouldBe(true);
    }

    [Test]
    public override void AuthorizeAttributeCheck()
    {
        var hasAuthorizeAttribute = HasAuthorizeAttribute<SendEmailConfirmationEmailCommand>();

        hasAuthorizeAttribute.ShouldBeTrue();
    }
}
