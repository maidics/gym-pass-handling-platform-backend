using FitPass.Domain.Entities;
using FitPass.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FitPass.Infrastructure.Data.Configurations;

public class GymMembershipConfiguration : IEntityTypeConfiguration<GymMembership>
{
    public void Configure(EntityTypeBuilder<GymMembership> builder)
    {
        builder
            .HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(gm => gm.ApplicationUserId);

        builder
            .HasOne(gm => gm.Gym)
            .WithOne()
            .HasForeignKey<GymMembership>(gm => gm.GymId)
            .OnDelete(DeleteBehavior.SetNull);

        builder
            .HasMany(gm => gm.Passes)
            .WithOne(p => p.GymMembership)
            .HasForeignKey(p => p.GymMembershipId);
    }
}
