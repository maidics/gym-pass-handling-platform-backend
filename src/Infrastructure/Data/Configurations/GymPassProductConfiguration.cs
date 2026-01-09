using FitPass.Domain.Entities;
using FitPass.Infrastructure.Common;
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

        builder.OwnsOne(gpp => gpp.Price).ConfigureMoney();
    }
}
