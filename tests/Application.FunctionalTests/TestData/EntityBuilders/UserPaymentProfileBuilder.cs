using FitPass.Application.FunctionalTests.TestData.Common;
using FitPass.Domain.Entities;
using FitPass.Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;

namespace FitPass.Application.FunctionalTests.TestData.EntityBuilders;

public class UserPaymentProfileBuilder : TestEntityBuilderBase<UserPaymentProfile>
{
    private string _id = Guid.NewGuid().ToString();
    private string? _applicationUserId;
    private string? _stripeCustomerId;
    private ICollection<PurchaseReceipt> _purchaseReceipts = [];

    public UserPaymentProfileBuilder(IServiceScopeFactory scopeFactory) : base(scopeFactory) { }

    public UserPaymentProfileBuilder WithId(string id)
    {
        AssertId(id);

        _id = id;

        return this;
    }

    public UserPaymentProfileBuilder WithApplicationUserId(string applicationUserId)
    {
        AssertId(applicationUserId);

        _applicationUserId = applicationUserId;

        return this;
    }

    public UserPaymentProfileBuilder WithStripeCustomerId(string stripeCustomerId)
    {
        AssertId(stripeCustomerId);

        _stripeCustomerId = stripeCustomerId;

        return this;
    }

    public UserPaymentProfileBuilder WithPurchaseReceipts(ICollection<PurchaseReceipt> purchaseReceipts)
    {
        _purchaseReceipts = purchaseReceipts;

        return this;
    }

    public UserPaymentProfileBuilder AddPurchaseReceipts(ICollection<PurchaseReceipt> purchaseReceipts)
    {
        _purchaseReceipts = [.. _purchaseReceipts, .. purchaseReceipts];

        return this;
    }

    public override UserPaymentProfile Build()
    {
        var userPaymentProfile = new UserPaymentProfile
        {
            ApplicationUserId = _applicationUserId,
            StripeCustomerId = _stripeCustomerId,
            PurchaseReceipts = _purchaseReceipts
        };

        return userPaymentProfile;
    }

    public override async Task<UserPaymentProfile> BuildAsync()
    {
        var userPaymentProfile = Build();

        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await context.UserPaymentProfiles.AddAsync(userPaymentProfile);
        await context.SaveChangesAsync();

        var createdUserPaymentProfile = await context.UserPaymentProfiles.FindAsync(userPaymentProfile.Id);

        Guard.Against.NotFound(userPaymentProfile.Id, createdUserPaymentProfile);

        return createdUserPaymentProfile;
    }

    protected override void AssertEntity()
    {
        throw new NotImplementedException();
    }
}
