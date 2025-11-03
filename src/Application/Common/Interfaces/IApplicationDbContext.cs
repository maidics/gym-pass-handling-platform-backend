using FitPass.Domain;
using FitPass.Domain.Entities;
using Microsoft.EntityFrameworkCore.Storage;

namespace FitPass.Application.Common.Interfaces;

public interface IApplicationDbContext
{    
    DbSet<UserPaymentProfile> UserPaymentProfiles { get; }
    DbSet<UserProfile> UserProfiles { get; }
    DbSet<Gym> Gyms { get; }
    DbSet<GymMembership> GymMemberships { get; }
    DbSet<GymEmployment> GymEmployments { get; }
    DbSet<GymMembershipPass> GymMembershipPasses { get; }
    DbSet<Request> Requests { get; }
    DbSet<GymPassProduct> GymPassProducts { get; }
    DbSet<PurchaseReceipt> PurchaseReceipts { get; }
    Task<int> SaveChangesAsync();
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
}
