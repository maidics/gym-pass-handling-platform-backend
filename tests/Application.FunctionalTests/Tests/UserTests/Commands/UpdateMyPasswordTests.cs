using FitPass.Application.FunctionalTests.Common.Extensions;
using FitPass.Application.Users.Commands;
using FitPass.Infrastructure.Identity;

namespace FitPass.Application.FunctionalTests.Tests.UserTests.Commands;

using static Testing;

public class UpdateMyPasswordTests : BaseTestFixture
{
    [Test]
    public override void AuthorizeAttributeCheck()
    {
        ShouldRequireAuthorization<UpdateMyPasswordCommand>();
    }

    [TestCase("", "Password123_", "Password123_")]
    [TestCase("Password123!", "", "Password123_")]
    [TestCase("Password123!", "Password123!", "Password123_")]
    [TestCase("Password123!", "Password123_", "")]
    [TestCase("Password123!", "Password123_", "Password123.")]
    [TestCase("Password123!", "Password123!", "Password123!")]
    public async Task ShouldThrowIfParametersAreInvalid(
        string currentPassword,
        string newPassword,
        string newPasswordConfirm
    )
    {
        await RunAsDefaultUserAsync();

        var command = new UpdateMyPasswordCommand(currentPassword, newPassword, newPasswordConfirm);

        await ShouldThrowIfParametersAreInvalidAsync(command);
    }

    [Test]
    public async Task ShouldUpdatePassword()
    {
        var obj = await RunAsDefaultUserAsync();

        var command = new UpdateMyPasswordCommand("Password123!", "Password123_", "Password123_");

        var result = await SendAsync(command);
        result.ShouldBeSuccessful();

        var updatedUser = await FindAsync<ApplicationUser>(obj.user.Id);
        updatedUser.ShouldNotBeNull();
        updatedUser.PasswordHash.ShouldNotBe(obj.user.PasswordHash);
    }
}
