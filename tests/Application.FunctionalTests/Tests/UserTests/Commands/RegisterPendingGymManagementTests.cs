using FitPass.Application.Common.Exceptions;
using FitPass.Application.Common.Models;
using FitPass.Application.Users.Commands;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using FitPass.Infrastructure.Identity;

namespace FitPass.Application.FunctionalTests.Tests.UserTests.Commands;

using static Testing;

public class RegisterPendingGymManagementTests : BaseTestFixture
{
    [Test]
    public override void AuthorizeAttributeCheck()
    {
        ShouldNotRequireAuthorization<RegisterPendingGymEmployeeCommand>();
    }

    [Test]
    public async Task ShouldDenyInvalidParameters()
    {
        var command = new RegisterPendingGymEmployeeCommand(string.Empty, string.Empty, string.Empty, "2", string.Empty);

        await Should.ThrowAsync<ValidationException>(SendAsync(command));
    }

    [Test]
    public async Task ShouldReturnConflictIfUserEmailIsInUser()
    {
        var user = await CreateUserAsync();

        var command = new RegisterPendingGymEmployeeCommand("First", "Last", user.Email!, "Password123_", "Password123_");

        var result = await SendAsync(command);
        result.Type.ShouldBe(ResultTypes.Conflict);
        result.Message.ShouldNotBeEmpty();
    }

    [Test]
    public async Task ShouldRegisterUser()
    {
        var firstName = "First";
        var lastName = "Last";
        var email = "email@test";

        var command = new RegisterPendingGymEmployeeCommand(firstName, lastName, email, "Password123!", "Password123!");

        var result = await SendAsync(command);
        result.Succeeded.ShouldBeTrue();
        result.Value.ShouldNotBeNull();

        var userId = await GetUserIdByEmailAsync(email);

        var user = await FindAsync<ApplicationUser>(userId);
        user.ShouldNotBeNull();
        user.PasswordHash.ShouldNotBeNull();
        var userRoles = await GetUserRolesAsync(userId);
        userRoles.Count.ShouldBe(1);
        userRoles.First().ShouldBe(Roles.PendingGymEmployee);

        var userProfile = await FindAsync<UserProfile>(userId);
        userProfile.ShouldNotBeNull();
        userProfile.FirstName.ShouldBe(firstName);
        userProfile.LastName.ShouldBe(lastName);
    }
}
