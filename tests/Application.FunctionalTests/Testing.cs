using System.Linq.Expressions;
using FitPass.Application.FunctionalTests.TestData;
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

    public static TestApplicationUserBuilder TestApplicationUserBuilder => new(_scopeFactory);
    public static TestGymBuilder TestGymBuilder => new(_scopeFactory);

    [OneTimeSetUp]
    public async Task RunBeforeAnyTests()
    {
        _database = await TestDatabaseFactory.CreateAsync();

        _factory = new CustomWebApplicationFactory(_database.GetConnection(), _database.GetConnectionString());

        _scopeFactory = _factory.Services.GetRequiredService<IServiceScopeFactory>();

        await SeedRolesIfNotExist();
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

    public static async Task<ApplicationUser> RunAsDefaultUserAsync()
    {
        var user = await TestApplicationUserBuilder.BuildAsync();

        return await RunAsUserAsync(user);
    }

    public static async Task<ApplicationUser> RunAsAppAdminAsync()
    {
        var user = await TestApplicationUserBuilder.WithRole(Roles.AppAdministrator).BuildAsync();

        return await RunAsUserAsync(user);
    }

    public static async Task<ApplicationUser> RunAsGymAdminAsync()
    {
        var user = await TestApplicationUserBuilder.WithRole(Roles.GymAdministrator).BuildAsync();

        return await RunAsUserAsync(user);
    }

    public static async Task<ApplicationUser> RunAsGymStaffAsync()
    {
        var user = await TestApplicationUserBuilder.WithRole(Roles.GymStaff).BuildAsync();

        return await RunAsUserAsync(user);
    }

    public static async Task<ApplicationUser> RunAsPendingGymManagementAsync()
    {
        var user = await TestApplicationUserBuilder.WithRole(Roles.PendingGymManagement).BuildAsync();

        return await RunAsUserAsync(user);
    }

    public static async Task<ApplicationUser> RunAsUserAsync(ApplicationUser user)
    {
        using var scope = _scopeFactory.CreateScope();

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var roles = await userManager.GetRolesAsync(user);

        _userId = user.Id;
        _roles = roles.ToList();

        return user;
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
        _roles = null;
    }

    public static void SetLoggedInUserId(string userId)
    {
        _userId = userId; 
    }

    public static async Task<IEnumerable<string>> GetUserRoleAsync(ApplicationUser user)
    {
        using var scope = _scopeFactory.CreateScope();

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        return await userManager.GetRolesAsync(user);
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

    private static async Task SeedRolesIfNotExist()
    {
        using var scope = _scopeFactory.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        var roles = new List<IdentityRole>()
        {
            new(Roles.AppAdministrator),
            new(Roles.GymAdministrator),
            new(Roles.GymStaff),
            new(Roles.PendingGymManagement)
        };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role.Name!))
            {
                var result = await roleManager.CreateAsync(role);

                if (!result.Succeeded)
                {
                    throw new InvalidOperationException($"Failed to create {role} role. Result: {result.ToApplicationResult()}");
                }
            }
        }
    }

    public static async Task<TEntity?> FindAsync<TEntity>(object[] keyValues, params Expression<Func<TEntity, object?>>[] includeProperties) where TEntity : class
    {
        using var scope = _scopeFactory.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        if (includeProperties.Length == 0)
        {
            return await context.FindAsync<TEntity>(keyValues);
        }

        IQueryable<TEntity> query = context.Set<TEntity>();
        
        foreach (var property in includeProperties)
        {
            query = query.Include(property);
        }

        return await FindByKeyAsync(context, query, keyValues);
    }

    private static async Task<TEntity?> FindByKeyAsync<TEntity>(ApplicationDbContext context, IQueryable<TEntity> query, object[] keyvalues) where TEntity : class
    {
        var entityType = context.Model.FindEntityType(typeof(TEntity));

        if (entityType == null)
        {
            throw new InvalidOperationException($"Entity type {typeof(TEntity).Name} not found in db context model.");
        }

        var keys = entityType.FindPrimaryKey()?.Properties;

        if (keys == null || keys.Count != keyvalues.Length)
        {
            throw new InvalidOperationException($"Number of key values ({keyvalues.Length}) does not match the number of key properties ({keys?.Count ?? 0}).");
        }

        var parameter = Expression.Parameter(typeof(TEntity), "e");

        Expression? predicate = null;

        for (int i = 0; i < keyvalues.Length; i++)
        {
            var property = keys[i];
            var propertyAccess = Expression.Property(parameter, property.Name);
            var keyValue = Expression.Constant(keyvalues[i]);
            var equals = Expression.Equal(propertyAccess, keyValue);

            predicate = predicate == null ? equals : Expression.AndAlso(predicate, equals);
        }

        if (predicate == null)
        {
            return null; 
        }

        var lambda = Expression.Lambda<Func<TEntity, bool>>(predicate, parameter);

        return await query.FirstOrDefaultAsync(lambda);
    }
}
