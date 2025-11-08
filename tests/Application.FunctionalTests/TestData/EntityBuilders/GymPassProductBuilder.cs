using FitPass.Application.FunctionalTests.TestData.Common;
using FitPass.Domain.Entities;
using FitPass.Domain.Enums;
using FitPass.Domain.ValueObjects;
using FitPass.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FitPass.Application.FunctionalTests.TestData.EntityBuilders;

public class GymPassProductBuilder : TestAuditableEntityBuilder<GymPassProductBuilder, GymPassProduct>
{
    private string _id = Guid.NewGuid().ToString();
    private string _gymId = string.Empty;
    private string _name = string.Empty;
    private string _description = string.Empty;
    private PassType _type = PassType.SingleUse;
    private int? _totalUses = 1;
    private int? _daysAfterExpiring;
    private bool _isActive = true;
    private Money _price = Money.Eur(2);
    private Gym? _gym;

    public GymPassProductBuilder(IServiceScopeFactory scopeFactory) : base(scopeFactory) { }

    public GymPassProductBuilder WithId(string id)
    {
        AssertId(id);

        _id = id;

        return this;
    }

    public GymPassProductBuilder WithGymId(string gymId)
    {
        AssertId(gymId);

        _gymId = gymId;

        return this;
    }

    public GymPassProductBuilder WithName(string name)
    {
        _name = name;

        return this;
    }

    public GymPassProductBuilder WithDescription(string description)
    {
        _description = description;

        return this;
    }

    public GymPassProductBuilder AsMultiUse(int totalUses)
    {
        _type = PassType.MultiUse;
        _totalUses = totalUses;

        _daysAfterExpiring = null;

        return this;
    }

    public GymPassProductBuilder AsUnlimitedUse(int daysAfterExpiring)
    {
        _type = PassType.Unlimited;
        _daysAfterExpiring = daysAfterExpiring;

        _totalUses = null;

        return this;
    }

    public GymPassProductBuilder WithPrice(Money price)
    {
        _price = price;

        return this;
    }

    public GymPassProductBuilder IsActive(bool isActive)
    {
        _isActive = isActive;

        return this;
    }

    public override GymPassProduct Build()
    {
        return new GymPassProduct
        {
            Id = _id,
            Name = _name,
            Description = _description,
            GymId = _gymId,
            Type = _type,
            Price = _price,
            TotalUses = _totalUses,
            DaysAfterExpiring = _daysAfterExpiring,
            IsActive = _isActive
        };
    }

    public override async Task<GymPassProduct> BuildAsync()
    {
        var gymPassProduct = Build();

        Guard.Against.NullOrEmpty(_gymId);
        
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await context.GymPassProducts.AddAsync(gymPassProduct);
        await context.SaveChangesAsync();

        var createdGymPassProduct = await context
            .GymPassProducts
            .Include(gpp => gpp.Gym)
            .FirstOrDefaultAsync(gpp => gpp.Id == _id);

        Guard.Against.Null(createdGymPassProduct);

        return createdGymPassProduct;
    }

    protected override void AssertEntity()
    {
        throw new NotImplementedException();
    }
}
