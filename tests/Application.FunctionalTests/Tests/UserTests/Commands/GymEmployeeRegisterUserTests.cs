using FitPass.Application.Common.Exceptions;
using FitPass.Application.Common.Models;
using FitPass.Application.FunctionalTests.Common.Extensions;
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

    [TestCase("", "First", "Last")]
    [TestCase("invalid@email", "First", "Last")]
    [TestCase("email@test.com", "", "Last")]
    [TestCase("email@test.com", "First", "")]
    public async Task ShouldThrowIfParametersAreInvalid(
        string email,
        string firstName,
        string lastName
    )
    {
        await RunAsGymEmployeeAsync(Roles.GymStaff);

        var command = new GymEmployeeRegisterUserCommand(email, firstName, lastName);

        await ShouldThrowIfParametersAreInvalidAsync(command);
    }

    [Test]
    public async Task ShouldReturnConflictIfEmailIsAlreadyInUser()
    {
        var user = await CreateUserAsync();

        await RunAsGymEmployeeAsync(Roles.GymStaff);

        var command = new GymEmployeeRegisterUserCommand(user.Email!, "First", "Last");

        var result = await SendAsync(command);
        result.ShouldBeFailed(ResultTypes.Conflict);
    }

    [Test]
    public async Task ShouldRegisterUser()
    {
        var gymStaffObj = await RunAsGymEmployeeAsync(Roles.GymStaff);

        var command = new GymEmployeeRegisterUserCommand("email@test.com", "First", "Last");

        var result = await SendAsync(command);
        result.ShouldBeSuccessful();

        var dto = result.Value;

        var user = await FindAsync<ApplicationUser>(dto.UserId);
        user.ShouldNotBeNull();
        user.Email.ShouldNotBeNull();
        user.Email.ShouldBe(command.Email);
        user.PasswordHash.ShouldBeNull();
        user.EmailConfirmed.ShouldBeFalse();

        var profile = await FindByUserIdAsync<UserProfile>(dto.UserId);
        profile.ShouldNotBeNull();
        profile.FirstName.ShouldBe(command.FirstName);
        profile.LastName.ShouldBe(command.LastName);

        var userRoles = await GetUserRolesAsync(dto.UserId);
        userRoles.Count.ShouldBe(1);
        userRoles.First().ShouldBe(Roles.User);

        var gymMembership = await FindAsync<GymMembership>(dto.Id);
        gymMembership.ShouldNotBeNull();
        gymMembership.GymId.ShouldBe(gymStaffObj.gym.Id);
        gymMembership.UserId.ShouldBe(dto.UserId);

        EmailFolderShouldContainEmails();
    }
}
