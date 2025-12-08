using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using FitPass.Domain.Entities.Payment;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FitPass.Infrastructure.Data.Configurations;

public class GymConfiguration : IEntityTypeConfiguration<Gym>
{
    public void Configure(EntityTypeBuilder<Gym> builder)
    {
        builder
            .HasMany(g => g.PassProducts)
            .WithOne(gp => gp.Gym)
            .HasForeignKey(gpp => gpp.GymId);

        builder.HasIndex(g => g.Name).IsUnique();

        builder.Property(g => g.Name).HasMaxLength(MaxStringLengths.Description);

        builder
            .HasOne(g => g.PaymentProfile)
            .WithOne(tpp => tpp.Gym)
            .HasForeignKey<TenantPaymentProfile>(tpp => tpp.GymId);

        builder.OwnsOne(g => g.Address);
    }
}
