using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using Microsoft.AspNetCore.Identity;

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
                    GymStaffAssigment = null,
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
                    GymStaffAssigment = new GymStaffAssigment {
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
                    GymStaffAssigment = new GymStaffAssigment
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
                    GymStaffAssigment = null,
                    PaymentProfile = new UserPaymentProfile 
                    {
                        UserId = "User1",
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
                    GymStaffAssigment = null,
                    PaymentProfile = new UserPaymentProfile 
                    {
                        UserId = "User2"
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
                    GymStaffAssigment = new GymStaffAssigment 
                    {
                        ApplicationUserId = "PendingGymAdmin",
                        GymId = null,
                        Role = Roles.PendingGymAdministrator
                    },
                    PaymentProfile = null
                },
                Roles.PendingGymAdministrator,
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
