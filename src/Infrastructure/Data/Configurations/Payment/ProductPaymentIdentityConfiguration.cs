using FitPass.Domain.Entities.Payment;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FitPass.Infrastructure.Data.Configurations.Payment;

public class ProductPaymentIdentityConfiguration : IEntityTypeConfiguration<ProductPaymentIdentity>
{
    public void Configure(EntityTypeBuilder<ProductPaymentIdentity> builder)
    {
        builder
            .HasOne(ppi => ppi.GymPassProduct)
            .WithOne(gpp => gpp.PaymentIdentity)
            .HasForeignKey<ProductPaymentIdentity>(ppi => ppi.GymPassProductId);

        builder.OwnsMany(x => x.ArchivedPaymentProviderPrices, y =>
        {
            y.ToTable("ArchivedPrices");
            y.WithOwner().HasForeignKey("OwnerId");
            y.Property(z => z.Id);
            y.Property(z => z.ArchivedOn);
        });
    }
}
