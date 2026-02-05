using FitPass.Application.Common.Models;
using FitPass.Application.FunctionalTests.Common.Extensions;
using FitPass.Application.FunctionalTests.Infrastructure.Testing;
using FitPass.Application.Users.Commands.Emails;

namespace FitPass.Application.FunctionalTests.Tests.UserTests.Commands.Emails;

using static Testing;

public class SendEmailConfirmationEmailTests : BaseTestFixture
{
    [Test]
    public override void AuthorizeAttributeCheck()
    {
        ShouldRequireAuthorization<SendEmailConfirmationEmailCommand>();
    }

    [Test]
    public async Task ShouldReturnBusinessRuleViolationWhenUserEmailIsConfirmed()
    {
        var user = await CreateUserAsync(emailConfirmed: true);

        await RunAsUserAsync(user);

        var result = await SendAsync(new SendEmailConfirmationEmailCommand());
        result.ShouldBeFailed(ResultTypes.BusinessRuleViolation);
    }

    [Test]
    public async Task ShouldReturnSuccessForUser()
    {
        await RunAsDefaultUserAsync();

        var result = await SendAsync(new SendEmailConfirmationEmailCommand());
        result.ShouldBeSuccessful();
    }
}
