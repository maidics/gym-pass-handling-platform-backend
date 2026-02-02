using FitPass.Application.FunctionalTests.Infrastructure.Testing;
using FitPass.Application.UserProfiles.Queries;

namespace FitPass.Application.FunctionalTests.Tests.UserProfileTests.Queries;

using static Testing;

public class GetMyUserProfileTests : BaseTestFixture
{
    [Test]
    public override void AuthorizeAttributeCheck()
    {
        ShouldRequireAuthorization<GetMyUserProfileQuery>();
    }

    [Test]
    public async Task ShouldReturnUserProfile()
    {
        var obj = await RunAsDefaultUserAsync();

        var dto = await SendAsync(new GetMyUserProfileQuery());
        dto.AssertTo(obj.userProfile, obj.user.Email!);
    }
}
