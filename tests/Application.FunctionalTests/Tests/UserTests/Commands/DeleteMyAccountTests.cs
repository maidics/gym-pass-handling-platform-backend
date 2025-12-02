using FitPass.Application.Common.Exceptions;
using FitPass.Application.Users.Commands;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;

namespace FitPass.Application.FunctionalTests.Tests.UserTests.Commands;

using static Testing;

public class DeleteMyAccountTests : BaseTestFixture
{
    [Test]
    public async Task ShouldThrowIfUserDoesNotExist()
    {
        var command = new DeleteMyAccountCommand();

        SetLoggedInUserId("invalidUserId");

        var action = () => SendAsync(command);

        await action.ShouldThrowAsync<Exception>();
    }

    [Test]
    public async Task ShouldDeleteDefaultUserAccount()
    {
        var obj = await RunAsDefaultUserAsync();

        var command = new DeleteMyAccountCommand();

        var result = await SendAsync(command);
        result.Succeeded.ShouldBeTrue();

        var deletedUserProfile = await FindAsync<UserProfile>(obj.userProfile.UserId);
        deletedUserProfile.ShouldBeNull();
    }

    [Test]
    public override void AuthorizeAttributeCheck()
    {
        ShouldRequireAuthorization<DeleteMyAccountCommand>(
            Roles.User, 
            Roles.PendingGymEmployee, 
            Roles.GymAdministrator, 
            Roles.GymStaff);
    }
}
