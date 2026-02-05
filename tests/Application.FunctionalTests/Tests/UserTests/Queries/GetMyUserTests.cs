using FitPass.Application.FunctionalTests.Infrastructure.Testing;
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

    [TestCase(Roles.User)]
    [TestCase(Roles.PendingGymEmployee)]
    [TestCase(Roles.AppAdministrator)]
    public async Task ShouldReturnUserForNonGymEmployee(string role)
    {
        var obj = await RunAsDefaultUserAsync();

        var query = new GetMyUserQuery();

        var dto = await SendAsync(query);
        dto.Id.ShouldBe(obj.user.Id);
        dto.Email.ShouldBe(obj.user.Email);
        dto.FirstName.ShouldBe(obj.userProfile.FirstName);
        dto.LastName.ShouldBe(obj.userProfile.LastName);
        dto.PreferredLanguage.ShouldBe(obj.userProfile.PreferredLanguage);
        dto.CreatedOn.ShouldBe(obj.userProfile.CreatedOn);

        dto.Roles.Length.ShouldBe(1);
        dto.Roles.First().ShouldBe(role);
        dto.GymId.ShouldBeNull();
        dto.GymEmploymentId.ShouldBeNull();
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task ShouldReturnUserForGymEmployee(bool isGymAdmin)
    {
        var obj = await RunAsGymEmployeeAsync(isGymAdmin ? Roles.GymAdministrator : Roles.GymStaff);

        var query = new GetMyUserQuery();

        var dto = await SendAsync(query);
        dto.Id.ShouldBe(obj.user.Id);
        dto.Email.ShouldBe(obj.user.Email);
        dto.FirstName.ShouldBe(obj.userProfile.FirstName);
        dto.LastName.ShouldBe(obj.userProfile.LastName);
        dto.PreferredLanguage.ShouldBe(obj.userProfile.PreferredLanguage);
        dto.CreatedOn.ShouldBe(obj.userProfile.CreatedOn);

        dto.Roles.Length.ShouldBe(1);
        dto.Roles.First().ShouldBe(isGymAdmin ? Roles.GymAdministrator : Roles.GymStaff);
        dto.GymId.ShouldBe(obj.gym.Id);
        dto.GymEmploymentId.ShouldBe(obj.gymEmployment.Id);
    }
}
