using FitPass.Application.Common.Exceptions;
using FitPass.Application.Common.Models;
using FitPass.Application.FunctionalTests.Infrastructure.Testing;
using FitPass.Application.Users.Commands;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using FitPass.Domain.Strings;
using FitPass.Infrastructure.Identity;

namespace FitPass.Application.FunctionalTests.Tests.UserTests.Commands;

using static Testing;

public class GymEmployeeRegisterUserTests : BaseTestFixture
{
    [Test]
    public override void AuthorizeAttributeCheck()
    {
        ShouldRequireAuthorization<GymEmployeeRegisterUserCommand>(
            Roles.GymAdministrator,
            Roles.GymStaff
        );
    }

    [Test]
    public async Task ShouldThrowIfParametersAreInvalid()
    {
        await RunAsGymEmployeeAsync(Roles.GymStaff);

        var command = new GymEmployeeRegisterUserCommand(
            "invalidEmail",
            string.Empty,
            string.Empty
        );

        await Should.ThrowAsync<ValidationException>(SendAsync(command));
    }

    [Test]
    public async Task ShouldReturnConflictIfEmailIsAlreadyInUser()
    {
        var user = await CreateUserAsync();

        var gymStaffObj = await RunAsGymEmployeeAsync(Roles.GymStaff);

        var command = new GymEmployeeRegisterUserCommand(user.Email!, "First", "Last");

        var result = await SendAsync(command);
        result.Type.ShouldBe(ResultTypes.Conflict);
    }

    [Test]
    public async Task ShouldRegisterUser()
    {
        var gymStaffObj = await RunAsGymEmployeeAsync(Roles.GymStaff);

        string email = "valid@email";
        string firstName = "First";
        string lastName = "Last";

        var command = new GymEmployeeRegisterUserCommand(email, firstName, lastName);

        var result = await SendAsync(command);
        result.Succeeded.ShouldBeTrue();
        result.Value.ShouldNotBeNull();

        var createdUserId = await GetUserIdByEmailAsync(email);

        var createdUser = await FindAsync<ApplicationUser>(createdUserId);
        createdUser.ShouldNotBeNull();
        var userRoles = await GetUserRolesAsync(createdUserId);
        userRoles.Count.ShouldBe(1);
        userRoles.First().ShouldBe(Roles.User);
        createdUser.PasswordHash.ShouldBeNull();
        createdUser.EmailConfirmed.ShouldBeFalse();

        var userProfile = await FindByUserIdAsync<UserProfile>(createdUserId);
        userProfile.ShouldNotBeNull();

        var gymMembership = await FindByUserIdAsync<GymMembership>(createdUserId);
        gymMembership.ShouldNotBeNull();
        gymMembership.GymId.ShouldBe(gymStaffObj.gym.Id);

        EmailFolderShouldContainEmails(1);
    }
}
