using FitPass.Domain;
using FitPass.Domain.Entities;
using Microsoft.EntityFrameworkCore.Storage;

namespace FitPass.Application.Common.Interfaces;

public interface IApplicationDbContext
{    DbSet<UserPaymentProfile> UserPaymentProfiles { get; }
    DbSet<NonRegisteredUser> NonRegisteredUsers { get; }
    DbSet<Gym> Gyms { get; }
    DbSet<UserGymMembership> UserGymMemberships { get; }
    DbSet<GymEmployment> GymEmployments { get; }
    DbSet<OwnedPass> OwnedPasses { get; }
    DbSet<Request> Requests { get; }
    DbSet<GymPassProduct> GymPassProducts { get; }
    DbSet<GymPassProductTemplate> GymPassProductTemplates { get; }
    DbSet<PurchaseReceipt> PurchaseReceipts { get; }
    Task<int> SaveChangesAsync();
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
}
