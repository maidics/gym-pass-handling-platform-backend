using System.Linq;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using FitPass.Domain.Enums;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FitPass.Infrastructure.Data;

public static class InitialiserExtensions
{
    public static async Task InitialiseDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var initialiser = scope.ServiceProvider.GetRequiredService<ApplicationDbContextInitialiser>();

        await initialiser.InitialiseAsync();
        await initialiser.SeedAsync();
    }
}

public class ApplicationDbContextInitialiser
{
    private readonly ILogger<ApplicationDbContextInitialiser> _logger;
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public ApplicationDbContextInitialiser(ILogger<ApplicationDbContextInitialiser> logger, ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _logger = logger;
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task InitialiseAsync()
    {
        try
        {
            // See https://jasontaylor.dev/ef-core-database-initialisation-strategies
            await _context.Database.EnsureDeletedAsync();
            await _context.Database.EnsureCreatedAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while initialising the database.");
            throw;
        }
    }

    public async Task SeedAsync()
    {
        try
        {
            await TrySeedAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }

    public async Task TrySeedAsync()
    {
        List<IdentityRole> roles =
        [
            new IdentityRole(Roles.AppAdministrator),
            new IdentityRole(Roles.GymAdministrator),
            new IdentityRole(Roles.GymStaff),
        ];

        var gymId = "localhostGymId";

        await SeedGymAsync(gymId);
        await SeedRolesAsync(roles);
        await SeedUsersAsync(gymId, roles);
    }

    private async Task SeedGymAsync(string gymId)
    {
        var gyms = await _context.Gyms.ToListAsync();

        if (gyms.Count == 0 || gyms.FirstOrDefault(g => g.Id == gymId) == null)
        {
            await _context.Gyms.AddAsync(new Gym
            {
                Id = gymId,
                Name = "LocalHostGym",
                Address = "Localhost",
                Status = GymStatus.Active,
                Tier = GymTier.MidRange
            });
        }
    }

    private async Task SeedRolesAsync(List<IdentityRole> roles)
    {
        var existingRoles = _roleManager.Roles;

        foreach (var role in roles)
        {
            if (existingRoles.All(r => r.Name != role.Name))
            {
                await _roleManager.CreateAsync(role);
            }
        }
    }

    private async Task SeedUsersAsync(string gymId, List<IdentityRole> roles)
    {
        List<(ApplicationUser user, string role, string password)> defaultUsers =
        [
            (
                new ApplicationUser {
                    Id = "AppAdminLocalhostId",
                    Email = "appadmin@localhost",
                    UserName = "AppAdmin",
                    FirstName = "AppAdmin",
                    UserGymMemberships = null,
                    GymStaffAssigment = null
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
                    FirstName = "GymAdmin",
                    UserGymMemberships = null,
                    GymStaffAssigment = new GymStaffAssigment {
                        ApplicationUserId = "GymAdminLocalhostId",
                        GymId = gymId,
                        EscalationEmail = "escalation@localhost"
                    }
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
                    FirstName = "gymstaff",
                    UserGymMemberships = null,
                    GymStaffAssigment = new GymStaffAssigment
                    {
                        ApplicationUserId = "GymStaffLocalhostId",
                        GymId = gymId,
                        EscalationEmail = "escalation@localhost"
                    }
                },
                Roles.GymStaff,
                "Password123_"
            ),
            (
                new ApplicationUser
                {
                    Id = "UserLocalhostId",
                    UserName = "User",
                    Email = "user@localhost",
                    FirstName = "user",
                    UserGymMemberships = [
                        new UserGymMembership {
                            Id = "UserGymMemberShipId",
                            UserId = "UserLocalhostId",
                            GymId = gymId
                        }
                    ],
                    GymStaffAssigment = null
                },
                string.Empty,
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

                if (roles.All(role => !string.IsNullOrWhiteSpace(role.Name)) && obj.role != string.Empty)
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
