using FitPass.Domain.Entities;
using FitPass.Domain.Entities.ContactInfos;
using FitPass.Domain.Entities.Payment;
using Microsoft.EntityFrameworkCore.Storage;

namespace FitPass.Application.Common.Interfaces;

public interface IApplicationDbContext
{    
    DbSet<UserProfile> UserProfiles { get; }
    DbSet<Gym> Gyms { get; }
    DbSet<GymMembership> GymMemberships { get; }
    DbSet<GymEmployment> GymEmployments { get; }
    DbSet<GymMembershipPass> GymMembershipPasses { get; }
    DbSet<GymPassUsage> GymPassUsages { get; }
    DbSet<Request> Requests { get; }
    DbSet<GymPassProduct> GymPassProducts { get; }
    
    DbSet<TenantPaymentProfile> TenantPaymentProfiles { get; }
    DbSet<ProductPaymentIdentity> PaymentProducts { get; }
    DbSet<PurchaseReceipt> PurchaseReceipts { get; }
    DbSet<ProductPaymentIdentity> ProductPaymentIdentities { get; }
    
    DbSet<ApplicationContactInfo> ApplicationContactInfos { get; }
    DbSet<GymContactInfo> GymContactInfos { get; }

    Task<int> SaveChangesAsync();
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
}
