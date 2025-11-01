using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using FitPass.Infrastructure.Identity;

namespace FitPass.Infrastructure.Data.DbSeed;

public partial class ApplicationDbContextInitialiser
{
    private async Task SeedUsersAsync()
    {
        List<(ApplicationUser user, string role, string password)> defaultUsers =
        [
            (
                new ApplicationUser {
                    Id = "AppAdminLocalhostId",
                    Email = "appadmin@localhost",
                    UserName = "AppAdmin"
                },
                Roles.AppAdministrator,
                "Password123_"
            ),
            (
                new ApplicationUser
                {
                    Id = "GymAdminLocalhostId",
                    Email = "gymadmin@localhost",
                    UserName = "GymAdmin"
                },
                Roles.GymAdministrator,
                "Password123_"
                ),
            (
                new ApplicationUser
                {
                    Id = "GymStaffLocalhostId",
                    UserName = "GymStaff",
                    Email = "gymstaff@localhost"
                },
                Roles.GymStaff,
                "Password123_"
            ),
            (
                new ApplicationUser
                {
                    Id = "User1",
                    UserName = "User1",
                    Email = "user1@localhost"
                },
                string.Empty,
                "Password123_"
            ),
            (
                new ApplicationUser
                {
                    Id = "User2",
                    UserName = "User2",
                    Email = "user2@localhost"
                },
                string.Empty,
                "Password123_"
            ),
            (
                new ApplicationUser 
                {
                    Id = "PendingGymAdmin",
                    UserName = "PendingGymAdmin1"
                },
                Roles.PendingGymEmployee,
                "Password123_"
            )
        ];

        var existingUsers = _userManager.Users;

        foreach (var obj in defaultUsers)
        {
            if (existingUsers.All(u => u.UserName != obj.user.UserName))
            {
                var result = await _userManager.CreateAsync(obj.user, obj.password);

                if (!result.Succeeded)
                {
                    throw new ArgumentException($"Failed to create user: {result.Errors}");
                }

                if (_roles.All(role => !string.IsNullOrWhiteSpace(role.Name)) && obj.role != string.Empty)
                {
                    var roleResult = await _userManager.AddToRoleAsync(obj.user, obj.role);

                    if (!roleResult.Succeeded)
                    {
                        throw new ArgumentException($"Failed to add {obj.user.Id} user to {obj.role} role: {roleResult.Errors}");
                    }
                }
            }
        }
    }

}
