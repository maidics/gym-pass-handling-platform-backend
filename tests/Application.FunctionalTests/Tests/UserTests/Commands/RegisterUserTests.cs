using FitPass.Application.Common.Models;
using FitPass.Application.FunctionalTests.Common.Extensions;

using FitPass.Application.Users.Commands;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using FitPass.Infrastructure.Identity;

namespace FitPass.Application.FunctionalTests.Tests.UserTests.Commands;

using static Testing;

public class RegisterUserTests : BaseTestFixture
{
    [Test]
    public override void AuthorizeAttributeCheck()
    {
        ShouldNotRequireAuthorization<RegisterUserCommand>();
    }

    [TestCase("", "Last", "email@test.com", "Password123!", "Password123!", "hu-HU")]
    [TestCase("First", "", "email@test.com", "Password123!", "Password123!", "hu-HU")]
    [TestCase("First", "Last", "", "Password123!", "Password123!", "hu-HU")]
    [TestCase("First", "Last", "invalid@email", "Password123!", "Password123!", "hu-HU")]
    [TestCase("First", "Last", "email@test.com", "", "Password123!", "hu-HU")]
    [TestCase("First", "Last", "email@test.com", "password", "Password123!", "hu-HU")]
    [TestCase("First", "Last", "email@test.com", "Password123_", "Password123!", "hu-HU")]
    [TestCase("First", "Last", "email@test.com", "Password123!", "", "hu-HU")]
    [TestCase("First", "Last", "email@test.com", "Password123!", "Password123", "hu-HU")]
    [TestCase("First", "Last", "email@test.com", "Password123!", "Password123", "")]
    [TestCase("First", "Last", "email@test.com", "Password123!", "Password123", "xx-XX")]
    public async Task ShouldThrowIfParametersAreInvalid(
        string firstName,
        string lastName,
        string email,
        string password,
        string passwordConfirm,
        string preferredLanguage
    )
    {
        var command = new RegisterUserCommand(
            firstName,
            lastName,
            email,
            password,
            passwordConfirm,
            false,
            preferredLanguage
        );

        await ShouldThrowIfParametersAreInvalidAsync(command);
    }

    [Test]
    public async Task ShouldReturnConflictIfEmailIsInUse()
    {
        var user = await CreateUserAsync();

        var command = new RegisterUserCommand(
            "First",
            "Last",
            user.Email!,
            "Password123!",
            "Password123!",
            false,
            "hu-HU"
        );

        var result = await SendAsync(command);
        result.ShouldBeFailed(ResultTypes.Conflict);
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task ShouldRegisterUser(bool asPendingGymEmployee)
    {
        var command = new RegisterUserCommand(
            "First",
            "Last",
            "email@test.com",
            "Password123!",
            "Password123!",
            asPendingGymEmployee,
            "hu-HU"
        );

        var result = await SendAsync(command);
        result.ShouldBeSuccessful();

        var jwt = result.Value;
        jwt.ShouldBeValid();

        var userId = await GetUserIdByEmailAsync(command.Email);

        var user = await FindAsync<ApplicationUser>(userId);
        user.ShouldNotBeNull();
        user.EmailConfirmed.ShouldBeFalse();
        user.PasswordHash.ShouldNotBeNull();

        var userProfile = await FindByUserIdAsync<UserProfile>(userId);
        userProfile.ShouldNotBeNull();
        userProfile.FirstName.ShouldBe(command.FirstName);
        userProfile.LastName.ShouldBe(command.LastName);

        var userRoles = await GetUserRolesAsync(userId);
        userRoles.Count.ShouldBe(1);
        userRoles.First().ShouldBe(asPendingGymEmployee ? Roles.PendingGymEmployee : Roles.User);

        EmailFolderShouldContainEmails();
    }
}
