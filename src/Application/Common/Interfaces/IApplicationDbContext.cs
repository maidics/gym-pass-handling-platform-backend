using FitPass.Domain;
using FitPass.Domain.Entities;
using Microsoft.EntityFrameworkCore.Storage;

namespace FitPass.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<ApplicationUser> Users { get; }
    DbSet<NonRegisteredUser> NonRegisteredUsers { get; }
    DbSet<Gym> Gyms { get; }
    DbSet<UserGymMembership> UserGymMemberships { get; }
    DbSet<GymStaffAssigment> GymStaffAssigments { get; }
    DbSet<OwnedPass> Passes { get; }
    DbSet<Request> Requests { get; }
    DbSet<GymPassProduct> GymPassProducts { get; }
    DbSet<GymPassProductTemplate> GymPassProductTemplates { get; }
    DbSet<PurchaseReceipt> PurchaseReceipts { get; }
    Task<int> SaveChangesAsync();
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
}
