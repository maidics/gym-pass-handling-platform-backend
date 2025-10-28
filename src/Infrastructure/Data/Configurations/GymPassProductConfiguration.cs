using FitPass.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FitPass.Infrastructure.Data.Configurations;

public class GymPassProductConfiguration : IEntityTypeConfiguration<GymPassProduct>
{
    public void Configure(EntityTypeBuilder<GymPassProduct> builder)
    {
        builder
            .HasOne(gpp => gpp.Gym)
            .WithMany(g => g.PassProducts)
            .HasForeignKey(gpp => gpp.GymId);

        builder.Property(gpp => gpp.HUFPrice).HasPrecision(18, 2);
    }
}
