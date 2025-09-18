using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FitPass.Infrastructure.Data.Configurations;

public class GymConfiguration : IEntityTypeConfiguration<Gym>
{
    public void Configure(EntityTypeBuilder<Gym> builder)
    {
        builder.HasMany(g => g.GymPassProducts).WithOne(gp => gp.Gym).HasForeignKey(g => g.GymId);

        builder.HasMany(g => g.UserGymMemberships).WithOne(ugm => ugm.Gym).HasForeignKey(g => g.GymId);

        builder.Property(g => g.Name).HasMaxLength(MaxStringLengths.Name);

        builder.Property(g => g.Address).HasMaxLength(MaxStringLengths.Address);

        builder.Property(g => g.OwnerName).HasMaxLength(MaxStringLengths.Name);
    }
}