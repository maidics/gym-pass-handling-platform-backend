using FitPass.Application.Users.Queries;
using FitPass.Domain.Constants;

namespace FitPass.Application.FunctionalTests.Tests.UserTests.Queries;

using static Testing;

public class GetMyUserTests
{
    [Test]
    public void ShouldRequireAuthorization()
    {
        ShouldRequireAuthorization<GetMyUserQuery>();
    }

    [Test]
    public async Task ShouldReturnUser()
    {
        var obj = await RunAsAppAdminAsync();

        var dto = await SendAsync(new GetMyUserQuery());
        dto.ShouldNotBeNull();
        
        dto.Id.ShouldBe(obj.user.Id);
        dto.FirstName.ShouldBe(obj.userProfile.FirstName);
        dto.LastName.ShouldBe(obj.userProfile.LastName);
        dto.Email.ShouldBe(obj.user.Email!);
        dto.PreferredLanguage.ShouldBe(obj.userProfile.PreferredLanguage);
        dto.CreatedOn.ShouldBe(obj.userProfile.CreatedOn);
        dto.Roles.Length.ShouldBe(1);
        dto.Roles[0].ShouldBe(Roles.AppAdministrator);
        dto.IsEmailConfirmed.ShouldBe(obj.user.EmailConfirmed);
    }
}
