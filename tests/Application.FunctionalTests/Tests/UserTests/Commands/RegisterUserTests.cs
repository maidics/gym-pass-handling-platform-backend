namespace FitPass.Application.FunctionalTests.Tests.UserTests.Commands;

using FitPass.Application.Common.Exceptions;
using FitPass.Application.Common.Models;
using FitPass.Application.Users.Commands;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using FitPass.Infrastructure.Identity;
using static Testing;

public class RegisterUserTests : BaseTestFixture
{
    [Test]
    public async Task ShouldDenyWithInvalidInput()
    {
        var command = new RegisterUserCommand(string.Empty, string.Empty, string.Empty, "pass1", "pass2");

        var action = () => SendAsync(command);

        await action.ShouldThrowAsync<ValidationException>();
    }

    [Test]
    public async Task ShouldReturnConflictIfEmailIsInUse()
    {
        var user = await CreateUserAsync();

        var command = new RegisterUserCommand("First", "Last", user.Email!, "Password123!", "Password123!");

        var result = await SendAsync(command);
        result.Type.ShouldBe(ResultTypes.Conflict);
        result.Message.ShouldContain("Email is already taken");
    }

    [Test]
    public async Task ShouldRegisterUser()
    {
        var email = "email@test";
        var firstName = "First";
        var lastName = "Last";

        var command = new RegisterUserCommand(firstName, lastName, email, "Password123!", "Password123!");

        var result = await SendAsync(command);
        result.Succeeded.ShouldBeTrue();
        result.Value.ShouldNotBeNull();

        var userId = await GetUserIdByEmailAsync("email@test");

        var user = await FindAsync<ApplicationUser>(userId);

        user.ShouldNotBeNull();

        var userProfile = await FindByApplicationUserIdAsync<UserProfile>(userId);

        userProfile.ShouldNotBeNull();

        userProfile.FirstName.ShouldBe(firstName);
        userProfile.LastName.ShouldBe(lastName);

        var userRoles = await GetUserRolesAsync(userId);

        userRoles.Count.ShouldBe(1);
        userRoles.First().ShouldBe(Roles.User);
    }
    
    public override void AuthorizeAttributeCheck()
    {
        ShouldNotRequireAuthorization<RegisterUserCommand>();
    }
}
