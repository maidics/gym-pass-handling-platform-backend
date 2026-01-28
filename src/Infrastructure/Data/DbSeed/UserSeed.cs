using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using FitPass.Infrastructure.Identity;

namespace FitPass.Infrastructure.Data.DbSeed;

public partial class ApplicationDbContextInitialiser
{
    private async Task SeedUsersAsync()
    {
        List<(ApplicationUser user, string role, string password, UserProfile profile)> users =
        [
            (
                new ApplicationUser {
                    Id = "AppAdminLocalhostId",
                    Email = "appadmin@localhost.com",
                    UserName = "AppAdmin"
                },
                Roles.AppAdministrator,
                "Password123!",
                new UserProfile
                {
                    FirstName = "App",
                    LastName = "Admin",
                    PreferredLanguage =  "hu-HU",
                    UserId = "AppAdminLocalhostId",
                    CreatedOn =  DateTime.UtcNow
                }
            ),
            (
                new ApplicationUser
                {
                    Id = "GymAdminLocalhostId",
                    Email = "gymadmin@localhost.com",
                    UserName = "GymAdmin"
                },
                Roles.GymAdministrator,
                "Password123!",
                new UserProfile
                {
                    FirstName = "Gym",
                    LastName = "Admin",
                    PreferredLanguage =  "hu-HU",
                    UserId = "GymAdminLocalhostId",
                    CreatedOn =  DateTime.UtcNow
                }
                ),
            (
                new ApplicationUser
                {
                    Id = "GymStaffLocalhostId",
                    UserName = "GymStaff",
                    Email = "gymstaff@localhost.com"
                },
                Roles.GymStaff,
                "Password123!",
                new UserProfile
                {
                    FirstName = "Gym",
                    LastName = "Staff",
                    PreferredLanguage =  "en-US",
                    UserId = "GymStaffLocalhostId",
                    CreatedOn =  DateTime.UtcNow
                }
            ),
            (
                new ApplicationUser
                {
                    Id = "UserId",
                    UserName = "User",
                    Email = "user@localhost.com"
                },
                Roles.User,
                "Password123!",
                new UserProfile
                {
                    FirstName = "User",
                    LastName = "User",
                    PreferredLanguage =  "hu-HU",
                    UserId = "UserId",
                    CreatedOn =  DateTime.UtcNow
                }
            ),
            (
                new ApplicationUser
                {
                    Id = "PendingGymEmployeeId",
                    UserName = "PendingGymEmployee",
                    Email = "pendinggymemployee@localhost.com"
                },
                Roles.PendingGymEmployee,
                "Password123!",
                new UserProfile
                {
                    FirstName = "Pending",
                    LastName = "GymEmployee",
                    PreferredLanguage =  "en-US",
                    UserId = "PendingGymEmployeeId",
                    CreatedOn = DateTime.UtcNow
                }
            )
        ];

        var existingUsers = _userManager.Users;

        foreach (var obj in users)
        {
            if (existingUsers.All(u => u.UserName != obj.user.UserName))
            {
                var result = await _userManager.CreateAsync(obj.user, obj.password);

                if (!result.Succeeded)
                {
                    throw new InvalidOperationException($"Failed to create user: {result.Errors}");
                }

                if (_roles.All(role => !string.IsNullOrWhiteSpace(role.Name)))
                {
                    var roleResult = await _userManager.AddToRoleAsync(obj.user, obj.role);

                    if (!roleResult.Succeeded)
                    {
                        throw new InvalidOperationException($"Failed to add {obj.user.Id} user to {obj.role} role: {string.Join(", ", roleResult.Errors)}");
                    }

                    var token = await _userManager.GenerateEmailConfirmationTokenAsync(obj.user);
                    
                    var emailResult = await _userManager.ConfirmEmailAsync(obj.user, token);

                    if (!emailResult.Succeeded)
                    {
                        throw new InvalidOperationException(
                            $"Failed to confirm {obj.user.Id} user email: {string.Join(", ", emailResult.Errors)}");
                    }
                }
                
                await _context.UserProfiles.AddAsync(obj.profile);
            }
        }

        await _context.SaveChangesAsync();
    }

}
