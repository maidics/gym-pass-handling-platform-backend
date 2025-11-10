using FitPass.Application.FunctionalTests.TestData;
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
        var obj = await TestEntityBuilder.BuildDefaultUserAsync();

        return (await RunAsUserAsync(obj.user), obj.userProfile);
    }

    public static async Task<(ApplicationUser user, UserProfile userProfile)> RunAsAppAdminAsync()
    {
        var obj = await TestEntityBuilder.BuildAppAdminAsync();

        return (await RunAsUserAsync(obj.user), obj.userProfile);
    }

    public static async Task<(ApplicationUser user, Gym gym, GymEmployment gymEmployment, UserProfile userProfile)> RunAsGymEmployeeAsync(string employeeRole)
    {
        var obj = await TestEntityBuilder.BuildGymEmployeeAsync(employeeRole);

        return (await RunAsUserAsync(obj.user), obj.gym, obj.gymEmployment, obj.userProfile);
    }

    public static async Task<(ApplicationUser user, UserProfile userProfile)> RunAsPendingGymEmployeeAsync()
    {
        var obj = await TestEntityBuilder.BuildPendingGymEmployeeAsync();

        return (await RunAsUserAsync(obj.user), obj.userProfile);
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
