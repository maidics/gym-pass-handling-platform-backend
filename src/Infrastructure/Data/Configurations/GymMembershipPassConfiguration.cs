using FitPass.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FitPass.Infrastructure.Data.Configurations;

public class OwnedPassConfiguration : IEntityTypeConfiguration<GymMembershipPass>
{
    public void Configure(EntityTypeBuilder<GymMembershipPass> builder)
    {
        builder
            .HasOne(gmp => gmp.GymMembership)
            .WithMany(gm => gm.Passes)
            .HasForeignKey(gmp => gmp.GymMembershipId);

        builder.Property(op => op.EurPrice).HasPrecision(18, 2);
    }
}
