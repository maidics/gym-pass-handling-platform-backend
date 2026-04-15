using System.Reflection;
using FitPass.Application.Common.Interfaces;
using FitPass.Domain.Entities;
using FitPass.Domain.Entities.Payment;
using FitPass.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace FitPass.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
    
    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
    public DbSet<Gym> Gyms => Set<Gym>();
    public DbSet<GymMembership> GymMemberships => Set<GymMembership>();
    public DbSet<GymEmployment> GymEmployments => Set<GymEmployment>();
    public DbSet<GymMembershipPass> GymMembershipPasses => Set<GymMembershipPass>();
    public DbSet<GymPassUsage> GymPassUsages => Set<GymPassUsage>();

    public DbSet<GymPassProduct> GymPassProducts => Set<GymPassProduct>();
    public DbSet<Request> Requests => Set<Request>();

    public DbSet<TenantPaymentProfile> TenantPaymentProfiles => Set<TenantPaymentProfile>();
    public DbSet<ProductPaymentIdentity> PaymentProducts => Set<ProductPaymentIdentity>();
    public DbSet<PurchaseReceipt> PurchaseReceipts => Set<PurchaseReceipt>();
    public DbSet<ProductPaymentIdentity> ProductPaymentIdentities => Set<ProductPaymentIdentity>();
    
    public DbSet<GymContactInfo> GymContactInfos => Set<GymContactInfo>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }

    public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        return await Database.BeginTransactionAsync(cancellationToken);
    }
}
