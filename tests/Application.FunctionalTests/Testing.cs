using System.Reflection;
using FitPass.Application.Common.Security;
using FitPass.Application.FunctionalTests.TestData.EntityBuilders;
using FitPass.Domain.Constants;
using FitPass.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
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

    public static ApplicationUserBuilder ApplicationUserBuilder => new(_scopeFactory);
    public static GymBuilder GymBuilder => new(_scopeFactory);
    public static GymEmploymentBuilder GymEmploymentBuilder => new(_scopeFactory);
    public static GymMembershipBuilder GymMembershipBuilder => new(_scopeFactory);
    public static GymMembershipPassBuilder GymMembershipPassBuilder => new(_scopeFactory);
    public static GymPassProductBuilder GymPassProductBuilder => new(_scopeFactory);
    public static GymPassUsageBuilder GymPassUsageBuilder => new(_scopeFactory);
    public static UserProfileBuilder UserProfileBuilder => new(_scopeFactory);
    public static RequestBuilder RequestBuilder => new(_scopeFactory);


    [OneTimeSetUp]
    public async Task RunBeforeAnyTests()
    {
        _database = await TestDatabaseFactory.CreateAsync();

        _factory = new CustomWebApplicationFactory(_database.GetConnection(), _database.GetConnectionString());

        _scopeFactory = _factory.Services.GetRequiredService<IServiceScopeFactory>();

        await SeedRolesIfNotExist();
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
}
