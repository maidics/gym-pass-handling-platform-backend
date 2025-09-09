using FitPass.Domain;
using FitPass.Domain.Entities;

namespace FitPass.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Gym> Gyms { get; }
    DbSet<UserGymMembership> UserGymMemberships { get; }
    DbSet<OwnedPass> Passes { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
