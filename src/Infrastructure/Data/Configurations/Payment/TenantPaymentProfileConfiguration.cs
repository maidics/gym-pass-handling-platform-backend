using FitPass.Domain.Entities.Payment;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FitPass.Infrastructure.Data.Configurations.Payment;

public class TenantPaymentProfileConfiguration : IEntityTypeConfiguration<TenantPaymentProfile>
{
    public void Configure(EntityTypeBuilder<TenantPaymentProfile> builder)
    {
        builder.OwnsOne(tpp => tpp.AccountStatus);
    }
}
