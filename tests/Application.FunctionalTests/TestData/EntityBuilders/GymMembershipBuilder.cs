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
    ICollection<GymMembershipPass> _passes = [];

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

    public GymMembershipBuilder WithPasses(ICollection<GymMembershipPass> passes)
    {
        _passes = passes;

        return this;
    }

    public GymMembershipBuilder AddPasses(ICollection<GymMembershipPass> passes)
    {
        _passes = [.. _passes, .. passes];

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
            Gym = _gym,
            Passes = _passes
        };

        ApplyAuditProperties(gymMembership);

        return gymMembership;
    }

    public override async Task<GymMembership> BuildAsync()
    {
        var gymMembership = Build();

        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        if (string.IsNullOrEmpty(gymMembership.ApplicationUserId))
        {
            var users = await context.Users.ToListAsync();

            if (users.Count == 0)
            {
                throw new InvalidOperationException("No ApplicationUserId was specified and no ApplicationUser exists.");
            }

            if (users.Count > 1)
            {
                throw new InvalidOperationException("No ApplicationUserId was specified and more than 1 ApplicationUser exists.");
            }

            gymMembership.ApplicationUserId = users.First().Id;
        }

        await context.GymMemberships.AddAsync(gymMembership);
        await context.SaveChangesAsync();

        var createdGymMembership = await context.GymMemberships.FindAsync(gymMembership.Id);

        Guard.Against.NotFound(gymMembership.Id, createdGymMembership);

        return createdGymMembership;
    }

    protected override void AssertEntity()
    {
        throw new NotImplementedException();
    }
}
