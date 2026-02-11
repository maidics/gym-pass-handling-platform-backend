using FitPass.Application.FunctionalTests.Common.Extensions;
using FitPass.Application.Users.Commands;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using FitPass.Infrastructure.Identity;

namespace FitPass.Application.FunctionalTests.Tests.UserTests.Commands;

using static Testing;

public class DeleteMyAccountTests : BaseTestFixture
{
    [Test]
    public override void AuthorizeAttributeCheck()
    {
        ShouldRequireAuthorization<DeleteMyAccountCommand>(
            Roles.User,
            Roles.PendingGymEmployee,
            Roles.GymAdministrator,
            Roles.GymStaff
        );
    }

    [Test]
    public async Task ShouldDeleteDefaultUserAccount()
    {
        var obj = await RunAsDefaultUserAsync();

        var command = new DeleteMyAccountCommand();

        var result = await SendAsync(command);
        result.ShouldBeSuccessful();

        var profile = await FindAsync<UserProfile>(obj.userProfile.Id);
        profile.ShouldBeNull();

        var user = await FindAsync<ApplicationUser>(obj.user.Id);
        user.ShouldBeNull();
    }
}
