
using FitPass.Application.UserProfiles.Commands;
using FitPass.Domain.Entities;

namespace FitPass.Application.FunctionalTests.Tests.UserProfileTests.Commands;

using static Testing;

public class UpdateMyPreferredLanguageTests : BaseTestFixture
{
    [Test]
    public override void AuthorizeAttributeCheck()
    {
        ShouldRequireAuthorization<UpdateMyPreferredLanguageCommand>();
    }

    [TestCase("")]
    [TestCase("LANG")]
    [TestCase("XX")]
    [TestCase("xx-XX")]
    public async Task ShouldThrowIfParametersAreInvalid(string newLanguage)
    {
        await RunAsDefaultUserAsync();

        var command = new UpdateMyPreferredLanguageCommand(newLanguage);

        await ShouldThrowIfParametersAreInvalidAsync(command);
    }

    [TestCase("hu-HU")]
    [TestCase("en-US")]
    public async Task ShouldUpdateMyPreferredLanguage(string newLanguage)
    {
        var obj = await RunAsDefaultUserAsync();

        var command = new UpdateMyPreferredLanguageCommand(newLanguage);

        await SendAsync(command);

        var profile = await FindAsync<UserProfile>(obj.userProfile.Id);
        profile.ShouldNotBeNull();
        profile.PreferredLanguage.ShouldBe(newLanguage);
    }
}
