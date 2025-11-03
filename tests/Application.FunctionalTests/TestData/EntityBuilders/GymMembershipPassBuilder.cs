using FitPass.Application.FunctionalTests.TestData.Common;
using FitPass.Domain.Entities;
using FitPass.Domain.Enums;
using FitPass.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FitPass.Application.FunctionalTests.TestData.EntityBuilders;

public class GymMembershipPassBuilder : TestAuditableEntityBuilder<GymMembershipPassBuilder, GymMembershipPass>
{
    private string _id = Guid.NewGuid().ToString();
    private string _gymMembershipId = string.Empty;
    private PassType _type = PassType.SingleUse;
    private int? _totalUses = 1;
    private int? _remainingUses = 1;
    private DateOnly? _expirationDate;
    private decimal _hufPrice = 100;
    private GymMembership? _gymMembership;

    public GymMembershipPassBuilder(IServiceScopeFactory scopeFactory) : base(scopeFactory) { }

    public GymMembershipPassBuilder WithId(string id)
    {
        AssertId(id);

        _id = id;

        return this;
    }

    public GymMembershipPassBuilder WithGymMembershipId(string gymMembershipId)
    {
        AssertId(gymMembershipId);

        _gymMembershipId = gymMembershipId;

        return this;
    }

    public GymMembershipPassBuilder AsMultiUseType(int totalUses, int? remainingUses = default)
    {
        if (remainingUses is not null && remainingUses > totalUses)
        {
            throw new InvalidOperationException("Remaining uses cannot be bigger than total uses.");
        }

        _type = PassType.MultiUse;
        _totalUses = totalUses;
        _remainingUses = remainingUses is null ? totalUses : remainingUses;

        _expirationDate = null;

        return this;
    }

    public GymMembershipPassBuilder AsUnlimitedUseType(DateOnly expirationDate)
    {
        _type = PassType.Unlimited;
        _expirationDate = expirationDate;

        _totalUses = null;
        _remainingUses = null;

        return this;
    }

    public GymMembershipPassBuilder WithPrice(decimal hufPrice)
    {
        if (hufPrice <= 0)
        {
            throw new InvalidOperationException("Pass price must be bigger than 0.");
        }

        _hufPrice = hufPrice;

        return this;
    }

    public GymMembershipPassBuilder WithGymMembership(GymMembership gymMembership)
    {
        _gymMembershipId = gymMembership.Id;
        _gymMembership = gymMembership;

        return this;
    }
    
    public GymMembershipPassBuilder FromGymPassProduct(GymPassProduct gymPassProduct)
    {
        _id = gymPassProduct.Id;
        _type = gymPassProduct.Type;
        _totalUses = gymPassProduct.TotalUses;
        _remainingUses = gymPassProduct.TotalUses;
        _expirationDate = gymPassProduct.GetExpirationDate();
        _hufPrice = gymPassProduct.HufPrice;

        return this;
    }

    public override GymMembershipPass Build()
    {
        var gymMembershipPass = new GymMembershipPass
        {
            Id = _id,
            GymMembershipId = _gymMembershipId,
            Type = _type,
            TotalUses = _totalUses,
            RemainingUses = _remainingUses,
            ExpirationDate = _expirationDate,
            HufPrice = _hufPrice
        };

        if (_gymMembership is not null)
        {
            gymMembershipPass.GymMembership = _gymMembership;
        }

        ApplyAuditProperties(gymMembershipPass);

        return gymMembershipPass;
    }

    public override async Task<GymMembershipPass> BuildAsync()
    {
        var gymMembershipPass = Build();

        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        if (string.IsNullOrEmpty(_gymMembershipId))
        {
            var gymMemberships = await context.GymMemberships.ToListAsync();

            if (gymMemberships.Count == 0)
            {
                throw new InvalidOperationException("No GymMembershipId was specified and no GymMembership exists.");
            }

            if (gymMemberships.Count > 1)
            {
                throw new InvalidOperationException("No GymMembershipId was specified and more than 1 GymMembership exists.");
            }

            gymMembershipPass.GymMembershipId = gymMemberships.First().Id;
            gymMembershipPass.GymMembership = gymMemberships.First();
        }

        await context.GymMembershipPasses.AddAsync(gymMembershipPass);
        await context.SaveChangesAsync();

        var createdGymMembershipPass = await context.GymMembershipPasses.FindAsync(gymMembershipPass.Id);

        Guard.Against.NotFound(gymMembershipPass.Id, createdGymMembershipPass);

        return createdGymMembershipPass;
    }

    protected override void AssertEntity()
    {
        throw new NotImplementedException();
    }
}
