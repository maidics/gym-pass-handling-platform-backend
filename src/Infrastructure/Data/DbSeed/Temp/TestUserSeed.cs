using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using FitPass.Infrastructure.Identity;

namespace FitPass.Infrastructure.Data.DbSeed;

public partial class ApplicationDbContextInitialiser
{
    private async Task SeedTestUsersAsync()
    {
        var now = DateTimeOffset.UtcNow;
        
        var users = new List<(ApplicationUser user, string role, UserProfile profile)>
        {
            (
                new ApplicationUser
                {
                    Id = "Passwordless",
                    UserName = "Passwordless",
                    Email = "passwordless@localhost"
                },
                Roles.User,
                new UserProfile
                {
                    UserId = "Passwordless",
                    FirstName = "Password",
                    LastName = "Less",
                    PreferredLanguage = "hu-HU",
                    CreatedOn = now
                }
            )
        };

        foreach (var obj in users)
        {
            var result = await _userManager.CreateAsync(obj.user);

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
