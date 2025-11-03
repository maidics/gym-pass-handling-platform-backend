using FitPass.Infrastructure.Stripe.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FitPass.Infrastructure.Data.Configurations;

public class StripeCustomerConfiguration : IEntityTypeConfiguration<StripeCustomer>
{
    public void Configure(EntityTypeBuilder<StripeCustomer> builder)
    {
        builder
            .HasOne(sc => sc.ApplicationUser)
            .WithOne()
            .HasForeignKey<StripeCustomer>(sc => sc.ApplicationUserId);
    }
}
