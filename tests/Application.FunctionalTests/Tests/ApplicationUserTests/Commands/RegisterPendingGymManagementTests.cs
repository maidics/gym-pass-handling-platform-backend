using FitPass.Application.ApplicationUsers.Commands;
using FitPass.Application.Common.Exceptions;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using FitPass.Infrastructure.Identity;

namespace FitPass.Application.FunctionalTests.Tests.ApplicationUserTests.Commands;

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
    public async Task ShouldThrowIfEmailIsInUse()
    {
        var user = await ApplicationUserBuilder.BuildAsync();

        var command = new RegisterPendingGymEmployeeCommand("First", "Last", user.Email!, "Password123_", "Password123_");

        var ex = await Should.ThrowAsync<ConflictException>(SendAsync(command));

        var conflictException = new ConflictException(nameof(RegisterPendingGymEmployeeCommand.Email));

        ex.Message.ShouldBe(conflictException.Message);
    }

    [Test]
    public async Task ShouldRegisterUser()
    {
        var firstName = "First";
        var lastName = "Last";
        var email = "valid@email";

        var command = new RegisterPendingGymEmployeeCommand(firstName, lastName, email, "Password123_", "Password123_");

        var jwtToken = await SendAsync(command);

        jwtToken.AccessToken.ShouldNotBeNull();

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
