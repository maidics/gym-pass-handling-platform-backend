using FitPass.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FitPass.Infrastructure.Data.Configurations;

public class GymConfiguration : IEntityTypeConfiguration<Gym>
{
    public void Configure(EntityTypeBuilder<Gym> builder)
    {
        builder.HasMany(g => g.GymPassProducts).WithOne(gp => gp.Gym).HasForeignKey(g => g.GymId);

        builder.HasMany(g => g.UserGymMemberships).WithOne(ugm => ugm.Gym).HasForeignKey(g => g.GymId);
    }
}