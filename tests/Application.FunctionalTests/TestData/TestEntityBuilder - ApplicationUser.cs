using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using FitPass.Infrastructure.Identity;

namespace FitPass.Application.FunctionalTests.TestData;

using static Testing;

public static partial class TestEntityBuilder
{
    public static async Task<(ApplicationUser user, UserProfile userProfile)> BuildDefaultUserAsync()
    {
        var user = await ApplicationUserBuilder
            .WithPassword("Password123_")
            .WithRole(Roles.User)
            .BuildAsync();

        var userProfile = await UserProfileBuilder
            .WithApplicationUserId(user.Id)
            .BuildAsync();

        return (user, userProfile);
    }

    public static async Task<(ApplicationUser user, UserProfile userProfile)> BuildPendingGymEmployeeAsync()
    {
        var user = await ApplicationUserBuilder
            .WithPassword("Password123_")
            .WithRole(Roles.PendingGymEmployee)
            .BuildAsync();

        var userProfile = await UserProfileBuilder
            .WithApplicationUserId(user.Id)
            .BuildAsync();

        return (user, userProfile);
    }

    public static async Task<(ApplicationUser user, UserProfile userProfile)> BuildAppAdminAsync()
    {
        var user = await ApplicationUserBuilder
            .WithPassword("Password123_")
            .WithRole(Roles.AppAdministrator)
            .BuildAsync();

        var userProfile = await UserProfileBuilder
            .WithApplicationUserId(user.Id)
            .BuildAsync();

        return (user, userProfile);
    }
    
    public static async Task<(ApplicationUser user, Gym gym, GymEmployment gymEmployment, UserProfile userProfile)> BuildGymEmployeeAsync(string employeeRole)
    {
        if (employeeRole != Roles.GymAdministrator && employeeRole != Roles.GymStaff)
        {
            throw new InvalidOperationException("$'{role}' role is not a gym employee role.");
        }

        var user = await ApplicationUserBuilder
            .WithPassword("Password123_")
            .WithRole(employeeRole)
            .BuildAsync();

        var gym = await GymBuilder
            .BuildAsync();

        var gymEmployment = await GymEmploymentBuilder
            .WithApplicationUserId(user.Id)
            .WithGymId(gym.Id)
            .WithRole(employeeRole)
            .BuildAsync();

        var userProfile = await UserProfileBuilder
            .WithApplicationUserId(user.Id)
            .BuildAsync();

        return (user, gym, gymEmployment, userProfile);
    }
}
