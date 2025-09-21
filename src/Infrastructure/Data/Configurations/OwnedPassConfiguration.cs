using FitPass.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FitPass.Infrastructure.Data.Configurations;

public class OwnedPassConfiguration : IEntityTypeConfiguration<OwnedPass>
{
    public void Configure(EntityTypeBuilder<OwnedPass> builder)
    {
        builder.HasOne(op => op.UserGymMembership).WithMany(ugm => ugm.OwnedPasses).HasForeignKey(ugm => ugm.Id);

        builder.Property(op => op.EurPrice).HasPrecision(18, 2);
    }
}
