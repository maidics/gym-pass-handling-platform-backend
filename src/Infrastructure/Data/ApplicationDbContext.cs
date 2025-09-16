using System.Reflection;
using FitPass.Application.Common.Interfaces;
using FitPass.Domain;
using FitPass.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace FitPass.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
    public DbSet<NonRegisteredUser> NonRegisteredUsers => Set<NonRegisteredUser>();
    public DbSet<Gym> Gyms => Set<Gym>();
    public DbSet<UserGymMembership> UserGymMemberships => Set<UserGymMembership>();
    public DbSet<GymStaffAssigment> GymStaffAssigments => Set<GymStaffAssigment>();
    public DbSet<OwnedPass> Passes => Set<OwnedPass>();
    public DbSet<GymPassProduct> GymPassProducts => Set<GymPassProduct>();
    public DbSet<Request> Requests => Set<Request>();
    public DbSet<PurchaseReceipt> PurchaseReceipts => Set<PurchaseReceipt>();

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
