using FitPass.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FitPass.Infrastructure.Data.Configurations;

public class StripePriceConfiguration : IEntityTypeConfiguration<PaymentPrice>
{
    public void Configure(EntityTypeBuilder<PaymentPrice> builder)
    {
        
    }
}
