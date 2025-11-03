using FitPass.Infrastructure.Stripe.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FitPass.Infrastructure.Data.Configurations;

public class StripeProductConfiguration : IEntityTypeConfiguration<StripeProduct>
{
    public void Configure(EntityTypeBuilder<StripeProduct> builder)
    {
        builder
            .HasOne(stripeProduct => stripeProduct.Price)
            .WithMany(stripePrice => stripePrice.Products)
            .HasForeignKey(stripeProduct => stripeProduct.StripePriceId);

        builder
            .HasOne(sp => sp.GymPassProduct)
            .WithOne()
            .HasForeignKey<StripeProduct>(sp => sp.GymPassProductId);
    }
}
