using FitPass.Application.Common.Interfaces;
using FitPass.Application.FunctionalTests.TestData.Common;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using FitPass.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace FitPass.Application.FunctionalTests.TestData;

public class TestGymBuilder : TestEntityBuilderBase<Gym>
{
    private readonly TestApplicationUserBuilder _testApplicationUserBuilder;
    private readonly Gym _gym;
    private bool _createGymAdmin = false;
    private bool _createGymStaff = false;
    private ApplicationUser? _gymAdmin = null;
    private ApplicationUser? _gymStaff = null;

    public TestGymBuilder(IServiceScopeFactory scopeFactory) : base(scopeFactory)
    {
        _testApplicationUserBuilder = new(scopeFactory);

        _gym = new Gym
        {
            Name = "Test Gym",
            Address = "Test Gym Address",
            Tier = GymTier.Local,
            Status = GymStatus.Active
        };
    }

    public TestGymBuilder WithMember(GymMembership gymMembership)
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

    public TestGymBuilder WithManagement(bool createGymAdmin = true, bool createGymStaff = true)
    {
        _createGymAdmin = createGymAdmin;
        _createGymStaff = createGymStaff;

        return this;
    }

    public ApplicationUser GetGymAdmin()
    {
        if (_gymAdmin == null)
        {
            throw new InvalidOperationException($"Call {nameof(WithManagement)} and {nameof(BuildAsync)} before calling this method to create a Gym Admin user.");
        }

        return _gymAdmin;
    }

    public ApplicationUser GetGymStaff()
    {
        if (_gymStaff == null)
        {
            throw new InvalidOperationException($"Call {nameof(WithManagement)} and {nameof(BuildAsync)} before calling this method to create a Gym Staff user.");
        }

        return _gymStaff;
    }

    private async Task CreateGymManagementIfNeeded()
    {
        if (_createGymAdmin)
        {
            _gymAdmin = await _testApplicationUserBuilder
                .WithRole(Roles.GymAdministrator)
                .WithGymStaffAssignment(Roles.GymAdministrator, _gym.Id)
                .BuildAsync();
        }

        if (_createGymStaff)
        {
            _gymStaff = await _testApplicationUserBuilder
                .WithRole(Roles.GymStaff)
                .WithGymStaffAssignment(Roles.GymStaff, _gym.Id)
                .BuildAsync();
        }
    }

    public override Gym Build()
    {
        return _gym;
    }

    public override async Task<Gym> BuildAsync()
    {
        using var scope = _scopeFactory.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

        await context.Gyms.AddAsync(_gym);
        await context.SaveChangesAsync();

        await CreateGymManagementIfNeeded();

        return _gym;
    }
}
