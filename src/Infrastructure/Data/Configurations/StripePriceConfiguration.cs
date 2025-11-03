using FitPass.Infrastructure.Stripe.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FitPass.Infrastructure.Data.Configurations;

public class StripePriceConfiguration : IEntityTypeConfiguration<StripePrice>
{
    public void Configure(EntityTypeBuilder<StripePrice> builder)
    {
        
    }
}
