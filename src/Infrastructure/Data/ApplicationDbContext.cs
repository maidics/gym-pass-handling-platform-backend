using System.Reflection;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Requests.DTOs;
using FitPass.Domain;
using FitPass.Domain.Entities;
using FitPass.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace FitPass.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
    public DbSet<Gym> Gyms => Set<Gym>();
    public DbSet<UserGymMembership> UserGymMemberships => Set<UserGymMembership>();
    public DbSet<GymStaffAssigment> GymStaffAssigments => Set<GymStaffAssigment>();
    public DbSet<OwnedPass> Passes => Set<OwnedPass>();
    public DbSet<GymPassProduct> GymPassProducts => Set<GymPassProduct>();
    public DbSet<Request<CreateGymDTO>> GymCreationRequests => Set<Request<CreateGymDTO>>();

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
