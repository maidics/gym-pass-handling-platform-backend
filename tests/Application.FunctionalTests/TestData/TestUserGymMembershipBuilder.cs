using FitPass.Application.FunctionalTests.TestData.Common;
using FitPass.Domain.Entities;
using FitPass.Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;

namespace FitPass.Application.FunctionalTests.TestData;

public class TestUserGymMembershipBuilder : TestEntityBuilderBase<UserGymMembership>
{
    private readonly UserGymMembership _userGymMembership;
    private readonly TestApplicationUserBuilder _testApplicationUserBuilder;
    private readonly TestGymBuilder _testGymBuilder;
    private bool _createApplicationUser = false;
    private bool _createNonRegisteredUser = false;
    private bool _createGym = false;
    private ApplicationUser? _applicationUser;
    private NonRegisteredUser? _nonRegisteredUser;
    private Gym? _gym;

    public TestUserGymMembershipBuilder(IServiceScopeFactory scopeFactory) : base(scopeFactory)
    {
        _testApplicationUserBuilder = new(scopeFactory);
        _testGymBuilder = new(scopeFactory);

        _userGymMembership = new UserGymMembership
        {
            ApplicationUserId = null,
            NonRegisteredUserId = null,
            GymId = null
        };
    }

    public TestUserGymMembershipBuilder ForApplicationUser(ApplicationUser user)
    {
        _userGymMembership.ApplicationUserId = user.Id;
        _userGymMembership.ApplicationUser = user;

        return this;
    }

    public TestUserGymMembershipBuilder ForNonRegisteredUser(NonRegisteredUser user)
    {
        _userGymMembership.NonRegisteredUserId = user.Id;
        _userGymMembership.NonRegisteredUser = user;

        return this;
    }

    public TestUserGymMembershipBuilder WithNavigationProperties(bool createApplicationUser, bool createNonRegisteredUser, bool createGym)
    {
        if (createApplicationUser && createNonRegisteredUser)
        {
            throw new InvalidOperationException("UserGymMembership can only belong to either ApplicationUser or NonRegisteredUser.");
        }

        _createApplicationUser = createApplicationUser;
        _createNonRegisteredUser = createNonRegisteredUser;
        _createGym = createGym;

        return this;
    }

    private async Task CreateNavigationPropertiesIfNeeded()
    {
        if (_createApplicationUser)
        {
            var user = await _testApplicationUserBuilder
                .AddUserGymMembership(_userGymMembership)
                .BuildAsync();

            _userGymMembership.ApplicationUserId = user.Id;
            _userGymMembership.ApplicationUser = user;

            _applicationUser = user;
        }

        if (_createNonRegisteredUser)
        {
            throw new NotImplementedException();
        }

        if (_createGym)
        {
            var gym = await _testGymBuilder.BuildAsync();

            _userGymMembership.GymId = gym.Id;
            _userGymMembership.Gym = gym;

            _gym = gym;
        }
    }

    public TestUserGymMembershipBuilder WithOwnedPass()
    {
        
    }

    public TestUserGymMembershipBuilder ForGym(Gym gym)
    {
        _userGymMembership.GymId = gym.Id;
        _userGymMembership.Gym = gym;

        return this;
    }

    public TestUserGymMembershipBuilder AddOwnedPass(params IEnumerable<OwnedPass> passes)
    {
        _userGymMembership.OwnedPasses.Concat(passes);

        return this;
    }

    public override UserGymMembership Build()
    {
        if (_userGymMembership.Gym == null || _userGymMembership.GymId == null)
        {
            throw new InvalidOperationException("No gym was set for UserGymMembership.");
        }

        if (_createApplicationUser || _createNonRegisteredUser || _createGym)
        {
            throw new InvalidOperationException($"{nameof(TestUserGymMembershipBuilder)}.{nameof(Build)} cannot create navigation properties, please use BuildAsync.");
        }

        return _userGymMembership;
    }

    public override async Task<UserGymMembership> BuildAsync()
    {
        await CreateNavigationPropertiesIfNeeded();

        var scope = _scopeFactory.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await context.UserGymMemberships.AddAsync(_userGymMembership);
        await context.SaveChangesAsync();

        return _userGymMembership;
    }

    private void AssertOwnedPass()
    {
        if (_userGymMembership.Gym == null || _userGymMembership.GymId == null)
        {
            throw new InvalidOperationException("No gym was set for UserGymMembership.");
        }

        if ()
    }
}
