using FitPass.Domain.Constants;
using FitPass.Infrastructure.Data.Interceptors;
using FitPass.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Stripe;

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
        await _factory.InitialiseStripeAsync();

        _scopeFactory = _factory.Services.GetRequiredService<IServiceScopeFactory>();

        await SeedRolesIfNotExist();

        using var scope = _scopeFactory.CreateScope();
        var stripeClient = scope.ServiceProvider.GetRequiredService<IStripeClient>();
        
        if (stripeClient.ApiKey != "sk_test_123")
        {
            throw new InvalidOperationException("Tests tried to run in non docker environment.");
        }
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
            new(Roles.User),
            new(Roles.AppAdministrator),
            new(Roles.GymAdministrator),
            new(Roles.GymStaff),
            new(Roles.PendingGymEmployee)
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

    public static async Task ResetState()
    {
        using var scope = _scopeFactory.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<InterceptorStateService>();

        service.IsAuditableEntityDisabled = true;

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

    public static DateTimeOffset GetUtcNow()
    {
        using var scope = _scopeFactory.CreateScope();
        var timeProvider = scope.ServiceProvider.GetRequiredService<TimeProvider>();
        return timeProvider.GetUtcNow();
    }
}
