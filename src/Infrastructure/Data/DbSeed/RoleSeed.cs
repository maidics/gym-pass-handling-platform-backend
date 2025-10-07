using FitPass.Domain.Constants;
using Microsoft.AspNetCore.Identity;

namespace FitPass.Infrastructure.Data.DbSeed;

public partial class ApplicationDbContextInitialiser
{
    private readonly IReadOnlyList<IdentityRole> _roles = [
            new IdentityRole(Roles.AppAdministrator),
            new IdentityRole(Roles.GymAdministrator),
            new IdentityRole(Roles.GymStaff),
            new IdentityRole(Roles.PendingGymManagement)
        ];

    private async Task SeedRolesAsync()
    {
        var existingRoles = _roleManager.Roles;

        foreach (var role in _roles)
        {
            if (existingRoles.All(r => r.Name != role.Name))
            {
                await _roleManager.CreateAsync(role);
            }
        }
    }
}
