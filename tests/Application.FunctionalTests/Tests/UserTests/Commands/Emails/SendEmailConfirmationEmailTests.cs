using FitPass.Application.Common.Exceptions;
using FitPass.Application.Common.Models;
using FitPass.Application.Users.Commands.Emails;
using FitPass.Domain.Constants;

namespace FitPass.Application.FunctionalTests.Tests.UserTests.Commands.Emails;

using static Testing;

public class SendEmailConfirmationEmailTests : BaseTestFixture
{
    [Test]
    public async Task ShouldReturnSuccessForUser()
    {
        await RunAsDefaultUserAsync();

        var result = await SendAsync(new SendEmailConfirmationEmailCommand());
        result.Type.ShouldBe(ResultTypes.Success);
    }

    [Test]
    public async Task ShouldReturnBusinessRuleViolationWhenUserEmailIsConfirmed()
    {
        var user = await CreateUserAsync(emailConfirmed: true);

        await RunAsUserAsync(user);

        var result = await SendAsync(new SendEmailConfirmationEmailCommand());
        result.Type.ShouldBe(ResultTypes.BusinessRuleViolation);
        result.Message.ShouldContain("Email is already confirmed");
    }

    [Test]
    public override void AuthorizeAttributeCheck()
    {
        ShouldRequireAuthorization<SendEmailConfirmationEmailCommand>();
    }
}
