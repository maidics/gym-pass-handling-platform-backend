using FitPass.Application.FunctionalTests.Common.Extensions;
using FitPass.Application.FunctionalTests.Infrastructure.Testing;
using FitPass.Application.UserProfiles.Commands;
using FitPass.Domain.Entities;

namespace FitPass.Application.FunctionalTests.Tests.UserProfileTests.Commands;

using static Testing;

public class UpdateMyUserProfileTests : BaseTestFixture
{
    [Test]
    public override void AuthorizeAttributeCheck()
    {
        ShouldRequireAuthorization<UpdateMyUserProfileCommand>();
    }

    [TestCase("", "Last", "hu-HU")]
    [TestCase("First", "", "hu-HU")]
    [TestCase("First", "Last", "")]
    [TestCase("First", "Last", "xx-XX")]
    public async Task ShouldThrowIfParametersAreInvalid(
        string firstName,
        string lastName,
        string preferredLanguage
    )
    {
        await RunAsDefaultUserAsync();

        var command = new UpdateMyUserProfileCommand(firstName, lastName, preferredLanguage);

        await ShouldThrowIfParametersAreInvalidAsync(command);
    }

    [TestCase("First", "Last", "hu-HU")]
    [TestCase("First", "Last", "en-US")]
    public async Task ShouldUpdateUserProfile(
        string firstName,
        string lastName,
        string preferredLanguage
    )
    {
        var obj = await RunAsDefaultUserAsync();

        var command = new UpdateMyUserProfileCommand(firstName, lastName, preferredLanguage);

        var result = await SendAsync(command);
        result.ShouldBeSuccessful();

        var profile = await FindAsync<UserProfile>(obj.userProfile.Id);
        profile.ShouldNotBeNull();
        profile.FirstName.ShouldBe(firstName);
        profile.LastName.ShouldBe(lastName);
        profile.PreferredLanguage.ShouldBe(preferredLanguage);
    }
}
