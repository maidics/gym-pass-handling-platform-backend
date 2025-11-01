using FitPass.Application.ApplicationUsers.Commands.Emails;
using FitPass.Application.Common.Exceptions;
using FitPass.Application.Common.Security;
using FitPass.Domain.Strings;

namespace FitPass.Application.FunctionalTests.ApplicationUsers.Commands;

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

        action.ShouldThrow<NotFoundException>();
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
    public override async Task AuthorizeAttributeCheck()
    {
        var hasAuthorizeAttribute = HasAuthorizeAttribute(typeof(SendEmailConfirmationEmailCommand));

        hasAuthorizeAttribute.ShouldBeTrue();
    }
}
