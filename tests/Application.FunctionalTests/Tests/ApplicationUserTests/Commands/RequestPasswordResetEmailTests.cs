using FitPass.Application.ApplicationUsers.Commands.Emails;
using FitPass.Application.Common.Exceptions;

namespace FitPass.Application.FunctionalTests.Tests.ApplicationUserTests.Commands;

using static Testing;

public class RequestPasswordResetEmailTests : BaseTestFixture
{
    [Test]
    public override void AuthorizeAttributeCheck()
    {
        var hasAuthorizeAttribute = HasAuthorizeAttribute<RequestPasswordResetEmailCommand>();

        hasAuthorizeAttribute.ShouldBeFalse();
    }

    [Test]
    public async Task ShouldDenyInvalidParameters()
    {
        var command = new RequestPasswordResetEmailCommand("invalidEmail");

        await Should.ThrowAsync<ValidationException>(SendAsync(command));
    }

    [Test]
    public async Task ShouldNotThrowIfUserDoesNotExist()
    {
        var command = new RequestPasswordResetEmailCommand("userNotExist@localhost");

        await Should.NotThrowAsync(SendAsync(command));
    }

    [Test]
    public async Task ShouldNotThrowIfUserExists()
    {
        var user = await ApplicationUserBuilder.BuildAsync();

        var command = new RequestPasswordResetEmailCommand(user.Email!);

        await Should.NotThrowAsync(SendAsync(command));
    }
}
