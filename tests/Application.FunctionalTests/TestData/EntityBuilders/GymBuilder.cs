using FitPass.Application.Common.Interfaces;
using FitPass.Application.FunctionalTests.TestData.Common;
using FitPass.Domain.Entities;
using FitPass.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FitPass.Application.FunctionalTests.TestData.EntityBuilders;

public class GymBuilder : TestAuditableEntityBuilder<GymBuilder, Gym>
{
    private string _id = Guid.NewGuid().ToString();
    private string _name = $"DefaultGym - {Guid.NewGuid()}";
    private string _address = "DefaultGymAddress";
    private GymStatus _status = GymStatus.Active;
    private GymTier _tier = GymTier.Local;
    private string? _ownerName;
    private ICollection<GymPassProduct> _passProducts = [];

    public GymBuilder(IServiceScopeFactory scopeFactory) : base(scopeFactory) { }

    public GymBuilder WithId(string id)
    {
        AssertId(id);

        _id = id;

        return this;
    }

    public GymBuilder WithName(string name)
    {
        _name = name;

        return this;
    }

    public GymBuilder WithAddress(string address)
    {
        _address = address; 
        
        return this; 
    }

    public GymBuilder WithStatus(GymStatus status)
    {
        _status = status;

        return this;
    }

    public GymBuilder WithTier(GymTier tier)
    {
        _tier = tier;

        return this;
    }

    public GymBuilder WithOwnerName (string? ownerName)
    {
        _ownerName = ownerName;

        return this;
    }

    public GymBuilder WithPassProducts(ICollection<GymPassProduct> passProducts)
    {
        _passProducts = passProducts;

        return this;
    }

    public GymBuilder AddPassProducts(ICollection<GymPassProduct> passProducts)
    {
        _passProducts = [.._passProducts, ..passProducts];

        return this;
    }

    public override Gym Build()
    {
        var gym = new Gym
        {
            Id = _id,
            Name = _name,
            Address = _address,
            Status = _status,
            Tier = _tier,
            OwnerName = _ownerName,
            PassProducts = _passProducts
        };

        ApplyAuditProperties(gym);

        return gym;
    }

    public override async Task<Gym> BuildAsync()
    {
        var gym = Build();

        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

        await context.Gyms.AddAsync(gym);
        await context.SaveChangesAsync();

        var createdGym = await context
            .Gyms
            .Include(g => g.PassProducts)
            .FirstOrDefaultAsync(g => g.Id == _id);

        Guard.Against.Null(createdGym);

        return createdGym;
    }

    protected override void AssertEntity()
    {
        throw new NotImplementedException();
    }
}
