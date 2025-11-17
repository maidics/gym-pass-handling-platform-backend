using FitPass.Domain.Entities.Payment;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FitPass.Infrastructure.Data.Configurations.Payment;

public class GymPaymentProfileConfiguration : IEntityTypeConfiguration<GymPaymentProfile>
{
    public void Configure(EntityTypeBuilder<GymPaymentProfile> builder)
    {
        builder.OwnsOne(gpp => gpp.BusinessAddress);

        builder.OwnsOne(gpp => gpp.Representative);
    }
}
