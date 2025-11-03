using FitPass.Application.FunctionalTests.TestData.Common;
using FitPass.Domain.Entities;
using FitPass.Domain.Enums;
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
    private decimal _hufPrice = 100;
    private bool _isActive = true;
    private bool _isCreatedOnStripe;
    private string? _stripePriceId;
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

    public GymPassProductBuilder WithPrice(decimal hufPrice)
    {
        if (hufPrice <= 0)
        {
            throw new InvalidOperationException("Pass price must be bigger than 0.");
        }

        _hufPrice = hufPrice;

        return this;
    }

    public GymPassProductBuilder IsActive(bool isActive)
    {
        _isActive = isActive;

        return this;
    }

    public override GymPassProduct Build()
    {
        var gymPassProduct = new GymPassProduct
        {

        };
    }

    public override Task<GymPassProduct> BuildAsync()
    {
        throw new NotImplementedException();
    }

    protected override void AssertEntity()
    {
        throw new NotImplementedException();
    }
}
