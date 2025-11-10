using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using FitPass.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace FitPass.Application.FunctionalTests;

public partial class Testing
{
    public static string? GetCurrentUserUserId()
    {
        return _userId;
    }

    public static List<string>? GetCurrentUserRoles()
    {
        return _roles;
    }

    public static async Task<(ApplicationUser user, UserProfile userProfile)> RunAsDefaultUserAsync()
    {
        var user = await ApplicationUserBuilder
            .WithPassword("Password123_")
            .BuildAsync();

        var userProfile = await UserProfileBuilder
            .WithApplicationUserId(user.Id)
            .BuildAsync();

        return (await RunAsUserAsync(user), userProfile);
    }

    public static async Task<(ApplicationUser user, UserProfile userProfile)> RunAsAppAdminAsync()
    {
        var user = await ApplicationUserBuilder
            .WithPassword("Password123_")
            .WithRole(Roles.AppAdministrator)
            .BuildAsync();

        var userProfile = await UserProfileBuilder
            .WithApplicationUserId(user.Id)
            .BuildAsync();

        return (await RunAsUserAsync(user), userProfile);
    }

    public static async Task<(ApplicationUser user, Gym gym, GymEmployment gymEmployment, UserProfile userProfile)> RunAsGymEmployeeAsync(string gymEmployeeRole)
    {
        if (gymEmployeeRole != Roles.GymAdministrator && gymEmployeeRole != Roles.GymStaff)
        {
            throw new InvalidOperationException("$'{role}' role is not a gym employee role.");
        }

        var user = await ApplicationUserBuilder
            .WithPassword("Password123_")
            .WithRole(gymEmployeeRole)
            .BuildAsync();

        var gym = await GymBuilder
            .BuildAsync();

        var gymEmployment = await GymEmploymentBuilder
            .WithApplicationUserId(user.Id)
            .WithGym(gym)
            .WithRole(gymEmployeeRole)
            .BuildAsync();

        var userProfile = await UserProfileBuilder
            .WithApplicationUserId(user.Id)
            .BuildAsync();

        return (await RunAsUserAsync(user), gym, gymEmployment, userProfile);
    }

    public static async Task<(ApplicationUser user, UserProfile userProfile)> RunAsPendingGymEmployeeAsync()
    {
        var user = await ApplicationUserBuilder
            .WithPassword("Password123_")
            .WithRole(Roles.PendingGymEmployee)
            .BuildAsync();

        var userProfile = await UserProfileBuilder
            .WithApplicationUserId(user.Id)
            .BuildAsync();

        return (await RunAsUserAsync(user), userProfile);
    }

    public static async Task<ApplicationUser> RunAsUserAsync(ApplicationUser user)
    {
        using var scope = _scopeFactory.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var roles = await userManager.GetRolesAsync(user);

        _userId = user.Id;
        _roles = roles.ToList();

        return user;
    }
    public static void SetLoggedInUserId(string userId)
    {
        _userId = userId;
    }
}
