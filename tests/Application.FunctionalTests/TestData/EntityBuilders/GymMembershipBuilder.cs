using FitPass.Application.FunctionalTests.TestData.Common;
using FitPass.Domain.Entities;
using FitPass.Domain.Enums;
using FitPass.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FitPass.Application.FunctionalTests.TestData.EntityBuilders;

public class GymMembershipBuilder : TestAuditableEntityBuilder<GymMembershipBuilder, GymMembership>
{
    private string _id = Guid.NewGuid().ToString();
    private string _applicationUserId = string.Empty;
    private string? _gymId;
    private GymMembershipStatus _status = GymMembershipStatus.Active;
    private Gym? _gym;
    //ICollection<GymMembershipPass> _passes = []; - cannot build a pass without the GymMembership first

    public GymMembershipBuilder(IServiceScopeFactory scopeFactory) : base(scopeFactory) { }

    public GymMembershipBuilder WithId(string id)
    {
        AssertId(id);

        _id = id;

        return this;
    }

    public GymMembershipBuilder WithApplicationUserId(string applicationUserId)
    {
        AssertId(applicationUserId);

        _applicationUserId = applicationUserId;

        return this;
    }

    public GymMembershipBuilder WithGymId(string gymId)
    {
        AssertId(gymId);

        _gymId = gymId;

        return this;
    }

    public GymMembershipBuilder WithStatus(GymMembershipStatus status)
    {
        _status = status;

        return this;
    }

    public GymMembershipBuilder WithGym(Gym gym)
    {
        _gymId = gym.Id;
        _gym = gym;

        return this;
    }

    public override GymMembership Build()
    {
        var gymMembership = new GymMembership
        {
            Id = _id,
            ApplicationUserId = _applicationUserId,
            Status = _status,
            GymId = _gymId,
            Gym = _gym
        };

        ApplyAuditProperties(gymMembership);

        return gymMembership;
    }

    public override async Task<GymMembership> BuildAsync()
    {
        var gymMembership = Build();

        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        Guard.Against.NullOrEmpty(_applicationUserId);

        await context.GymMemberships.AddAsync(gymMembership);
        await context.SaveChangesAsync();

        var createdGymMembership = await context
            .GymMemberships
            .Include(gm => gm.Gym)
            .Include(gm => gm.Passes)
            .FirstOrDefaultAsync(gm => gm.Id == _id);

        Guard.Against.Null(createdGymMembership);

        return createdGymMembership;
    }

    protected override void AssertEntity()
    {
        throw new NotImplementedException();
    }
}
