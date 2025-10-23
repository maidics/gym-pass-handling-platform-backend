using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using FitPass.Infrastructure.Data;
using FitPass.Infrastructure.Identity;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FitPass.Application.FunctionalTests;

[SetUpFixture]
public partial class Testing
{
    private static ITestDatabase _database = null!;
    private static CustomWebApplicationFactory _factory = null!;
    private static IServiceScopeFactory _scopeFactory = null!;
    private static string? _userId;
    private static List<string>? _roles;

    [OneTimeSetUp]
    public async Task RunBeforeAnyTests()
    {
        _database = await TestDatabaseFactory.CreateAsync();

        _factory = new CustomWebApplicationFactory(_database.GetConnection(), _database.GetConnectionString());

        _scopeFactory = _factory.Services.GetRequiredService<IServiceScopeFactory>();
    }

    public static async Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request)
    {
        using var scope = _scopeFactory.CreateScope();

        var mediator = scope.ServiceProvider.GetRequiredService<ISender>();

        return await mediator.Send(request);
    }

    public static async Task SendAsync(IBaseRequest request)
    {
        using var scope = _scopeFactory.CreateScope();

        var mediator = scope.ServiceProvider.GetRequiredService<ISender>();

        await mediator.Send(request);
    }

    public static string? GetUserId()
    {
        return _userId;
    }
    
    public static List<string>? GetRoles()
    {
        return _roles;
    }

    public static async Task<string> RunAsDefaultUserAsync()
    {
        var user = new ApplicationUser
        {
            Id = "DefaultUserId",
            FirstName = "Default",
            LastName = "User",
            Email = "default@localhost",
            GymStaffAssignment = null,
            UserGymMemberships = [
                    new UserGymMembership {
                        ApplicationUserId = "DefaultUserId",
                        NonRegisteredUserId = null,
                        GymId = "LocalhostGymId1"
                    }
                ]
        };

        return await RunAsUserAsync(user, "Password123_", []);
    }

    public static async Task<string> RunAsAppAdministratorAsync()
    {
        var user = new ApplicationUser
        {
            FirstName = "App",
            LastName = "Administrator",
            Email = "appadmin@localhost",
            GymStaffAssignment = null,
            UserGymMemberships = null
        };

        return await RunAsUserAsync(user, "Password123_", [Roles.AppAdministrator]);
    }

    public static async Task<string> RunAsGymAdministratorAsync()
    {
        var user = new ApplicationUser
        {
            Id = "GymAdministratorId",
            FirstName = "Gym",
            LastName = "Administrator",
            Email = "gymadmin@localhost",
            GymStaffAssignment = new GymStaffAssignment
            {
                ApplicationUserId = "GymAdministratorId",
                GymId = "LocalhostGymId1",
                Role = Roles.GymAdministrator
            },
            UserGymMemberships = null
        };

        return await RunAsUserAsync(user, "Password123_", [Roles.GymAdministrator]);
    }

    public static async Task<string> RunAsGymStaffAsync()
    {
        var user = new ApplicationUser
        {
            Id = "GymStaffId",
            FirstName = "Gym",
            LastName = "Staff",
            Email = "gymstaff@localhost",
            GymStaffAssignment = new GymStaffAssignment
            {
                ApplicationUserId = "GymStaffId",
                GymId = "LocalhostGymId1",
                Role = Roles.GymStaff
            },
            UserGymMemberships = null
        };

        return await RunAsUserAsync(user, "Password123_", [Roles.GymStaff]);
    }

    public static async Task<string> RunAsPendingGymManagementAsync()
    {
        var user = new ApplicationUser
        {
            FirstName = "Pending",
            LastName = "GymManagement",
            Email = "pendinggymmanagement@localhost",
            GymStaffAssignment = null,
            UserGymMemberships = null
        };

        return await RunAsUserAsync(user, "Password123_", [Roles.PendingGymManagement]);
    }

    public static async Task<string> RunAsUserAsync(ApplicationUser user, string password, string[] roles)
    {
        using var scope = _scopeFactory.CreateScope();

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var result = await userManager.CreateAsync(user, password);

        if (roles.Any())
        {
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            foreach (var role in roles)
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }

            await userManager.AddToRolesAsync(user, roles);
        }

        if (result.Succeeded)
        {
            _userId = user.Id;
            _roles = roles.ToList();
            return _userId;
        }

        var errors = string.Join(Environment.NewLine, result.ToApplicationResult().Errors);

        throw new Exception($"Unable to create {roles[0]} user.{Environment.NewLine}{errors}");
    }

    public static async Task ResetState()
    {
        try
        {
            await _database.ResetAsync();
        }
        catch (Exception) 
        {
        }

        _userId = null;
    }

    public static async Task<TEntity?> FindAsync<TEntity>(params object[] keyValues)
        where TEntity : class
    {
        using var scope = _scopeFactory.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        return await context.FindAsync<TEntity>(keyValues);
    }

    public static async Task AddAsync<TEntity>(TEntity entity)
        where TEntity : class
    {
        using var scope = _scopeFactory.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        context.Add(entity);

        await context.SaveChangesAsync();
    }

    public static async Task<int> CountAsync<TEntity>() where TEntity : class
    {
        using var scope = _scopeFactory.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        return await context.Set<TEntity>().CountAsync();
    }

    [OneTimeTearDown]
    public async Task RunAfterAnyTests()
    {
        await _database.DisposeAsync();
        await _factory.DisposeAsync();
    }
}
