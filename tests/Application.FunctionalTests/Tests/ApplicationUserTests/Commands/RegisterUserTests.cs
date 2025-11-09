namespace FitPass.Application.FunctionalTests.Tests.ApplicationUserTests.Commands;

using FitPass.Application.ApplicationUsers.Commands;
using FitPass.Application.ApplicationUsers.DTOs;
using FitPass.Application.Common.Exceptions;
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
    public async Task ShouldThrowIfEmailIsInUse()
    {
        var existingUser = await ApplicationUserBuilder.BuildAsync();

        var command = new RegisterUserCommand("First", "Last", existingUser.Email!, "Password123_", "Password123_");

        await Should.ThrowAsync<ConflictException>(SendAsync(command));
    }

    [Test]
    public async Task ShouldRegisterUser()
    {
        var email = "valid@email";
        var firstName = "First";
        var lastName = "Last";

        var command = new RegisterUserCommand(firstName, lastName, email, "Password123_", "Password123_");

        var jwtToken = await SendAsync(command);

        jwtToken.AccessToken.ShouldNotBeNull();

        var userId = await GetUserIdByEmailAsync("valid@email");

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
