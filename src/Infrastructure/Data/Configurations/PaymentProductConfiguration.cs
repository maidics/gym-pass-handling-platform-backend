using FitPass.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FitPass.Infrastructure.Data.Configurations;

public class StripeProductConfiguration : IEntityTypeConfiguration<PaymentProduct>
{
    public void Configure(EntityTypeBuilder<PaymentProduct> builder)
    {
        builder
            .HasOne(stripeProduct => stripeProduct.Price)
            .WithMany(stripePrice => stripePrice.Products)
            .HasForeignKey(stripeProduct => stripeProduct.StripePriceId);

        builder
            .HasOne(sp => sp.GymPassProduct)
            .WithOne()
            .HasForeignKey<PaymentProduct>(sp => sp.GymPassProductId);
    }
}
