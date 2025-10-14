using FitPass.Domain.Constants;
using FitPass.Domain.Entities;

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
                    UserName = "AppAdmin",
                    FirstName = "App",
                    LastName = "Admin",
                    UserGymMemberships = null,
                    GymStaffAssignment = null,
                    PaymentProfile = null
                },
                Roles.AppAdministrator,
                "Password123_"
            ),
            (
                new ApplicationUser
                {
                    Id = "GymAdminLocalhostId",
                    Email = "gymadmin@localhost",
                    UserName = "GymAdmin",
                    FirstName = "Gym",
                    LastName = "Admin",
                    UserGymMemberships = null,
                    GymStaffAssignment = new GymStaffAssignment {
                        ApplicationUserId = "GymAdminLocalhostId",
                        GymId = gymId1,
                        EscalationEmail = "escalation@localhost",
                        Role = Roles.GymAdministrator
                    },
                    PaymentProfile = null
                },
                Roles.GymAdministrator,
                "Password123_"
                ),
            (
                new ApplicationUser
                {
                    Id = "GymStaffLocalhostId",
                    UserName = "GymStaff",
                    Email = "gymstaff@localhost",
                    FirstName = "Gym",
                    LastName = "Staff",
                    UserGymMemberships = null,
                    GymStaffAssignment = new GymStaffAssignment
                    {
                        ApplicationUserId = "GymStaffLocalhostId",
                        GymId = gymId1,
                        EscalationEmail = "escalation@localhost",
                        Role = Roles.GymStaff
                    },
                    PaymentProfile = null
                },
                Roles.GymStaff,
                "Password123_"
            ),
            (
                new ApplicationUser
                {
                    Id = "User1",
                    UserName = "User1",
                    Email = "user1@localhost",
                    FirstName = "Localhost",
                    LastName = "User1",
                    UserGymMemberships = [],
                    GymStaffAssignment = null,
                    PaymentProfile = new UserPaymentProfile 
                    {
                        ApplicationUserId = "User1",
                        NonRegisteredUserId = null
                    }
                },
                string.Empty,
                "Password123_"
            ),
            (
                new ApplicationUser
                {
                    Id = "User2",
                    UserName = "User2",
                    Email = "user2@localhost",
                    FirstName = "Localhost",
                    LastName = "User2",
                    UserGymMemberships = [],
                    GymStaffAssignment = null,
                    PaymentProfile = new UserPaymentProfile 
                    {
                        ApplicationUserId = "User2",
                        NonRegisteredUserId = null
                    }
                },
                string.Empty,
                "Password123_"
            ),
            (
                new ApplicationUser 
                {
                    Id = "PendingGymAdmin",
                    UserName = "PendingGymAdmin1",
                    FirstName = "Pending",
                    LastName = "GymAdmin",
                    Email = "pendinggymadmin@localhost",
                    UserGymMemberships = null,
                    GymStaffAssignment = new GymStaffAssignment 
                    {
                        ApplicationUserId = "PendingGymAdmin",
                        GymId = null,
                        Role = Roles.PendingGymManagement
                    },
                    PaymentProfile = null
                },
                Roles.PendingGymManagement,
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
                        throw new ArgumentException($"Failed to add {obj.user.FirstName} user to {obj.role} role: {roleResult.Errors}");
                    }
                }
            }
        }
    }

}
