using FitPass.Domain.Entities.Payment;
using FitPass.Infrastructure.Common;
using FitPass.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FitPass.Infrastructure.Data.Configurations.Payment;

public class PurchaseReceiptConfiguration : IEntityTypeConfiguration<PurchaseReceipt>
{
    public void Configure(EntityTypeBuilder<PurchaseReceipt> builder)
    {
        builder.HasOne<ApplicationUser>().WithMany().HasForeignKey(pr => pr.UserId);

        builder.OwnsOne(pr => pr.Spent).ConfigureMoney();
    }
}
