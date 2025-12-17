using FitPass.Application.UserProfiles.Commands;
using FitPass.Domain.Entities;

namespace FitPass.Application.FunctionalTests.Tests.UserProfileTests.Commands;

using static Testing;

public class UpdateUserProfilePreferredLanguageTests : BaseTestFixture
{
    [Test]
    public override void AuthorizeAttributeCheck()
    {
        ShouldRequireAuthorization<UpdateUserProfilePreferredLanguageCommand>();
    }

    [TestCase("")]
    [TestCase("language")]
    [TestCase("...")]
    [TestCase("es-HU")]
    [TestCase("de-DE")]
    public async Task ShouldDenyInvalidParameters(string language)
    {
        await RunAsDefaultUserAsync();
        
        var command = new UpdateUserProfilePreferredLanguageCommand(language);

        await ShouldThrowIfParametersAreInvalidAsync(command);
    }

    [TestCase("en-US")]
    [TestCase("hu-HU")]
    public async Task ShouldUpdateLanguage(string language)
    {
        var obj = await RunAsDefaultUserAsync();

        var command = new UpdateUserProfilePreferredLanguageCommand(language);
        
        await SendAsync(command);

        var profile = await FindAsync<UserProfile>(obj.userProfile.Id);
        profile.ShouldNotBeNull();
        profile.PreferredLanguage.ShouldBe(language);
    }
}
