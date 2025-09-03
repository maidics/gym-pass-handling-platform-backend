using FitPass.Domain;
using FitPass.Domain.Entities;

namespace FitPass.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Gym> Gyms { get; }
    DbSet<Pass> Passes { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
