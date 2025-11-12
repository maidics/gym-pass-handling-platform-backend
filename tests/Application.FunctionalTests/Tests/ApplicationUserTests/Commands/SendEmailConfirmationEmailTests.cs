using FitPass.Application.ApplicationUsers.Commands.Emails;
using FitPass.Application.Common.Exceptions;
using FitPass.Domain.Constants;

namespace FitPass.Application.FunctionalTests.Tests.ApplicationUserTests.Commands;

using static Testing;

public class SendEmailConfirmationEmailTests : BaseTestFixture
{
    [Test]
    public async Task ShouldDenyInvalidParameters()
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
    public async Task ShouldReturnSuccessForUser()
    {
        var user = await ApplicationUserBuilder
            .WithEmailConfirmed(false)
            .WithPassword("Password123_")
            .BuildAsync();

        await RunAsUserAsync(user);

        var command = new SendEmailConfirmationEmailCommand(user.Email!);

        var result = await SendAsync(command);

        result.Succeeded.ShouldBe(true);
    }

    [Test]
    public async Task ShouldReturnSuccessForGymAdmin()
    {
        await RunAsGymEmployeeAsync(Roles.GymAdministrator);

        var user = await ApplicationUserBuilder
            .WithEmailConfirmed(false)
            .BuildAsync();

        var command = new SendEmailConfirmationEmailCommand(user.Email!);

        var result = await SendAsync(command);

        result.Succeeded.ShouldBeTrue();
    }

    [Test]
    public async Task ShouldDenySendingEmailForOtherUsersForNonGymEmployee()
    {
        await RunAsAppAdminAsync();

        var user = await ApplicationUserBuilder.BuildAsync();

        var command = new SendEmailConfirmationEmailCommand(user.Email!);

        await Should.ThrowAsync<ForbiddenAccessException>(SendAsync(command));
    }

    [Test]
    public override void AuthorizeAttributeCheck()
    {
        ShouldRequireAuthorization<SendEmailConfirmationEmailCommand>();
    }
}
