using FitPass.Application.ApplicationUsers.Commands;
using FitPass.Application.Common.Exceptions;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using FitPass.Domain.Strings;
using FitPass.Infrastructure.Identity;

namespace FitPass.Application.FunctionalTests.Tests.ApplicationUserTests.Commands;

using static Testing;

public class GymEmployeeRegisterUserTests : BaseTestFixture
{
    [Test]
    public override void AuthorizeAttributeCheck()
    {
        ShouldRequireAuthorization<GymEmployeeRegisterUserCommand>(Roles.GymAdministrator, Roles.GymStaff);
    }

    [Test]
    public async Task ShouldDenyInvalidParameters()
    {
        await RunAsGymAdminAsync();

        var command = new GymEmployeeRegisterUserCommand("invalidEmail", string.Empty, string.Empty);

        await Should.ThrowAsync<ValidationException>(SendAsync(command));
    }

    [Test]
    public async Task ShouldThrowIfUserAlreadyExists()
    {
        var gymStaff = await RunAsGymStaffAsync();

        var gym = await GymBuilder.BuildAsync();

        var gymEmployment = await GymEmploymentBuilder.WithApplicationUserId(gymStaff.Id).WithGymId(gym.Id).BuildAsync();

        var user = await ApplicationUserBuilder.BuildAsync();

        var command = new GymEmployeeRegisterUserCommand(user.Email!, "First", "Last");

        var ex = await Should.ThrowAsync<ConflictException>(SendAsync(command));

        var conflictException = new ConflictException(nameof(GymEmployeeRegisterUserCommand.Email));

        ex.Message.ShouldBe(conflictException.Message);
    }

    [Test]
    public async Task ShouldRegisterUser()
    {
        var gymStaff = await RunAsGymStaffAsync();

        var gym = await GymBuilder.BuildAsync();

        var gymEmployment = await GymEmploymentBuilder.WithApplicationUserId(gymStaff.Id).WithGymId(gym.Id).BuildAsync();

        string email = "valid@email";
        string firstName = "First";
        string lastName = "Last";

        var command = new GymEmployeeRegisterUserCommand(email, firstName, lastName);

        var gymMembershipDto = await SendAsync(command);

        var createdUserId = await GetUserIdByEmailAsync(email);

        var createdUser = await FindAsync<ApplicationUser>(createdUserId);
        createdUser.ShouldNotBeNull();
        var userRoles = await GetUserRolesAsync(createdUserId);
        userRoles.Count.ShouldBe(1);
        userRoles.First().ShouldBe(Roles.User);
        createdUser.PasswordHash.ShouldBeNull();
        createdUser.EmailConfirmed.ShouldBeFalse();

        var userProfile = await FindByApplicationUserIdAsync<UserProfile>(createdUserId);
        userProfile.ShouldNotBeNull();

        var gymMembership = await FindByApplicationUserIdAsync<GymMembership>(createdUserId);
        gymMembership.ShouldNotBeNull();
        gymMembership.GymId.ShouldBe(gym.Id);
    }
}
