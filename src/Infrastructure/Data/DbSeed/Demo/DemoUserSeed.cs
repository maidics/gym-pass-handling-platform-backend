using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using FitPass.Infrastructure.Identity;

namespace FitPass.Infrastructure.Data.DbSeed;

public partial class ApplicationDbContextInitialiser
{
    private async Task SeedTestUsersAsync()
    {
        var now = DateTimeOffset.UtcNow;
        
        var users = new List<(ApplicationUser user, string role, string? password, UserProfile profile)>
        {
            (
                new ApplicationUser
                {
                    Id = "Passwordless",
                    Email = "passwordless@localhost.com",
                    UserName = "passwordless@localhost.com"
                },
                Roles.User,
                null,
                new UserProfile
                {
                    UserId = "Passwordless",
                    FirstName = "Jelszómentes",
                    LastName = "János",
                    PreferredLanguage = "hu-HU",
                    CreatedOn = now
                }
            ),
            (
                new ApplicationUser
                {
                    Id = "GymAdminLocalhostId",
                    Email = "gymadmin@localhost.com",
                    UserName = "gymadmin@localhost.com",
                },
                Roles.GymAdministrator,
                "Password123!",
                new UserProfile
                {
                    FirstName = "Edzőtermi Adminisztrátor",
                    LastName = "Elek",
                    PreferredLanguage = "hu-HU",
                    UserId = "GymAdminLocalhostId",
                    CreatedOn = DateTime.UtcNow,
                }
            ),
            (
                new ApplicationUser
                {
                    Id = "GymStaffLocalhostId",
                    Email = "gymstaff@localhost.com",
                    UserName = "gymstaff@localhost.com",
                },
                Roles.GymStaff,
                "Password123!",
                new UserProfile
                {
                    FirstName = "Edzőtermi Alkalmazott",
                    LastName = "Ernő",
                    PreferredLanguage = "en-US",
                    UserId = "GymStaffLocalhostId",
                    CreatedOn = DateTime.UtcNow,
                }
            ),
            (
                new ApplicationUser
                {
                    Id = "UserId",
                    Email = "user@localhost.com",
                    UserName = "user@localhost.com"
                },
                Roles.User,
                "Password123!",
                new UserProfile
                {
                    FirstName = "Felhasználó",
                    LastName = "Ferenc",
                    PreferredLanguage = "hu-HU",
                    UserId = "UserId",
                    CreatedOn = DateTime.UtcNow,
                }
            ),
            (
                new ApplicationUser
                {
                    Id = "PendingGymEmployeeId",
                    Email = "pending@localhost.com",
                    UserName = "pending@localhost.com",
                },
                Roles.PendingGymEmployee,
                "Password123!",
                new UserProfile
                {
                    FirstName = "Várakozó",
                    LastName = "Vera",
                    PreferredLanguage = "en-US",
                    UserId = "PendingGymEmployeeId",
                    CreatedOn = DateTime.UtcNow,
                }
            ),
        };

        foreach (var obj in users)
        {
            var result = obj.password is null
                ? await _userManager.CreateAsync(obj.user)
                : await _userManager.CreateAsync(obj.user, password: obj.password);

            if (!result.Succeeded)
            {
                throw new ArgumentException($"Failed to create user: {result.Errors}");
            }

            if (_roles.All(role => !string.IsNullOrWhiteSpace(role.Name)))
            {
                var roleResult = await _userManager.AddToRoleAsync(obj.user, obj.role);

                if (!roleResult.Succeeded)
                {
                    throw new ArgumentException($"Failed to add {obj.user.Id} user to {obj.role} role: {roleResult.Errors}");
                }
            }
            
            await _context.UserProfiles.AddAsync(obj.profile);
        }
    }
}
