using FitPass.Application.Requests.DTOs;
using FitPass.Domain;
using FitPass.Domain.Entities;
using Microsoft.EntityFrameworkCore.Storage;

namespace FitPass.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<ApplicationUser> Users { get; }
    DbSet<Gym> Gyms { get; }
    DbSet<UserGymMembership> UserGymMemberships { get; }
    DbSet<GymStaffAssigment> GymStaffAssigments { get; }
    DbSet<OwnedPass> Passes { get; }
    DbSet<Request<CreateGymDTO>> GymCreationRequests { get; }
    DbSet<GymPassProduct> GymPassProducts { get; }
    DbSet<PurchaseReceipt> PurchaseReceipts { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
}
