using FitPass.Application.Common.Interfaces;
using FitPass.Application.FunctionalTests.TestData.Common;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using FitPass.Domain.Enums;
using FitPass.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace FitPass.Application.FunctionalTests.TestData;

public class TestGymBuilder : TestEntityBuilderBase<Gym>
{
    private readonly Gym _gym;

    private ApplicationUser? _gymAdmin = null;
    private ApplicationUser? _gymStaff = null;

    private string? _gymManagement = null;

    public TestGymBuilder(IServiceScopeFactory scopeFactory) : base(scopeFactory)
    {
        _gym = new()
        {
            Name = "Test Gym",
            Address = "Test Gym Address",
            Tier = GymTier.Local,
            Status = GymStatus.Active
        };
    }

    public TestGymBuilder WithMember(UserGymMembership gymMembership)
    {
        _gym.UserGymMemberships.Add(gymMembership);
        return this;
    }

    public TestGymBuilder WithStatus(GymStatus gymStatus)
    {
        _gym.Status = gymStatus;
        return this;
    }

    public TestGymBuilder WithOwnerName(string ownerName)
    {
        _gym.OwnerName = ownerName;
        return this;
    }

    public async Task<TestGymBuilder> WithManagement()
    {
        _gymManagement = "both";

        return this;
    }

    public async Task<TestGymBuilder> WithGymAdmin()
    {
        _gymManagement = Roles.GymAdministrator;

        return this;
    }

    public async Task<TestGymBuilder> WithGymStaff()
    {
        _gymManagement = Roles.GymStaff;

        return this;
    }

    public ApplicationUser GetGymAdmin()
    {
        if (_gymAdmin == null)
        {
            throw new InvalidOperationException($"Call ({nameof(WithManagement)} or {nameof(WithGymAdmin)}) and {nameof(BuildAsync)} before calling this method to create a Gym Admin user.");
        }

        return _gymAdmin;
    }

    public ApplicationUser GetGymStaff()
    {
        if (_gymStaff == null)
        {
            throw new InvalidOperationException($"Call ({nameof(WithManagement)} or {nameof(WithGymStaff)}) and {nameof(BuildAsync)} before calling this method to create a Gym Staff user.");
        }

        return _gymStaff;
    }

    private async Task<ApplicationUser> CreateGymManagementUser(string role, UserManager<ApplicationUser> userManager)
    {
        var gymManagementUserId = Guid.NewGuid().ToString();

        var gymManagementUser = new ApplicationUser
        {
            Id = gymManagementUserId,
            FirstName = "Gym",
            LastName = role,
            Email = $"{role}_{gymManagementUserId}@localhost",
            UserName = $"{role}_{gymManagementUserId}@localhost",
            GymStaffAssignment = new GymStaffAssignment
            {
                ApplicationUserId = gymManagementUserId,
                GymId = _gym.Id,
                Role = role
            },
            UserGymMemberships = null,
            PaymentProfile = null,
        };

        var creationResult = await userManager.CreateAsync(gymManagementUser, "Password123_");

        if (!creationResult.Succeeded)
        {
            throw new InvalidOperationException($"Failed to create {role} user with {nameof(TestGymBuilder)}. Result: {creationResult.ToApplicationResult()}");
        }

        var roleResult = await userManager.AddToRoleAsync(gymManagementUser, role);

        if (!roleResult.Succeeded)
        {
            throw new InvalidOperationException($"Failed to add user to {role} role {nameof(TestGymBuilder)}. Result: {creationResult.ToApplicationResult()}");
        }

        return gymManagementUser;
    }

    public override Gym Build()
    {
        return _gym;
    }

    public override async Task<Gym> BuildAsync()
    {
        using var scope = _scopeFactory.CreateScope();

        if (_gymManagement != null)
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            if (_gymManagement == "both")
            {
                _gymAdmin = await CreateGymManagementUser(Roles.GymAdministrator, userManager);
                _gymStaff = await CreateGymManagementUser(Roles.GymStaff, userManager);
            }

            if (_gymManagement == Roles.GymAdministrator)
            {
                _gymAdmin = await CreateGymManagementUser(Roles.GymAdministrator, userManager);
            }

            if (_gymManagement == Roles.GymStaff)
            {
                _gymAdmin = await CreateGymManagementUser(Roles.GymStaff, userManager);
            }
        }

        var context = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

        await context.Gyms.AddAsync(_gym);
        await context.SaveChangesAsync();
        return _gym;
    }
}
