using FitPass.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FitPass.Infrastructure.Data.Configurations;

public class UserGymMembershipConfiguration : IEntityTypeConfiguration<UserGymMembership>
{
    public void Configure(EntityTypeBuilder<UserGymMembership> builder)
    {
        builder.HasKey(ugm => new { ugm.UserId, ugm.GymId });

        builder.HasOne(ugm => ugm.Gym).WithMany(ugm => ugm.UserGymMemberships).HasForeignKey(ugm => ugm.GymId);

        builder.HasMany(ugm => ugm.OwnedPasses).WithOne(op => op.UserGymMembership).HasForeignKey(op => op.UserGymMembershipId);

        builder.HasOne(ugm => ugm.ApplicationUser).WithMany(au => au.UserGymMemberships);
    }
}