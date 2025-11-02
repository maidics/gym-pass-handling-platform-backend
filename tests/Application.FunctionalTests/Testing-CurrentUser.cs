using FitPass.Domain.Constants;
using FitPass.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace FitPass.Application.FunctionalTests;

public partial class Testing
{
    public static string? GetUserId()
    {
        return _userId;
    }

    public static List<string>? GetRoles()
    {
        return _roles;
    }

    public static async Task<ApplicationUser> RunAsDefaultUserAsync()
    {
        var user = await ApplicationUserBuilder.BuildAsync();

        return await RunAsUserAsync(user);
    }

    public static async Task<ApplicationUser> RunAsAppAdminAsync()
    {
        var user = await ApplicationUserBuilder.WithRole(Roles.AppAdministrator).BuildAsync();

        return await RunAsUserAsync(user);
    }

    public static async Task<ApplicationUser> RunAsGymAdminAsync()
    {
        var user = await ApplicationUserBuilder.WithRole(Roles.GymAdministrator).BuildAsync();

        return await RunAsUserAsync(user);
    }

    public static async Task<ApplicationUser> RunAsGymStaffAsync()
    {
        var user = await ApplicationUserBuilder.WithRole(Roles.GymStaff).BuildAsync();

        return await RunAsUserAsync(user);
    }

    public static async Task<ApplicationUser> RunAsPendingGymEmployeeAsync()
    {
        var user = await ApplicationUserBuilder.WithRole(Roles.PendingGymEmployee).BuildAsync();

        return await RunAsUserAsync(user);
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
