using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
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
                QRCode = [],
                Name = "LocalHostGym",
                Address = "Localhost"
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
        List<(ApplicationUser, string)> defaultUsers =
        [
            (
                new ApplicationUser {
                    Id = "AppAdminLocalhostId",
                    FirstName = "AppAdmin",
                    UserGymMemberships = null,
                    GymStaffAssigment = null
                },
                Roles.AppAdministrator
            ),
            (
                new ApplicationUser
                {
                    Id = "GymAdminLocalhostId",
                    Email = "gymadmin@localhost",
                    FirstName = "GymAdmin",
                    UserGymMemberships = null,
                    GymStaffAssigment = new GymStaffAssigment {
                        ApplicationUserId = "GymAdminLocalhostId",
                        GymId = gymId,
                        EscalationEmail = "escalation@localhost"
                    }
                },
                Roles.GymAdministrator
                ),
            (
                new ApplicationUser
                {
                    Id = "GymStaffLocalhostId",
                    UserName = "gymstaff@localhost",
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
                Roles.GymStaff
            ),
            (
                new ApplicationUser
                {
                    Id = "UserLocalhostId",
                    UserName = "user@localhost",
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
                string.Empty
            )
        ];

        var existingUsers = _userManager.Users;

        foreach (var obj in defaultUsers)
        {
            if (existingUsers.All(u => u.UserName != obj.Item1.UserName))
            {
                await _userManager.CreateAsync(obj.Item1, obj.Item1.FirstName);

                if (roles.All(role => !string.IsNullOrWhiteSpace(role.Name)) && obj.Item2 != string.Empty)
                {
                    await _userManager.AddToRolesAsync(obj.Item1, [obj.Item2]);
                }
            }
        }
    }
}
