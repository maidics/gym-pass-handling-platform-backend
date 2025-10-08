using FitPass.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fitpass.Infrastructure.Data.Configurations;

public class NonRegisteredUserConfiguration : IEntityTypeConfiguration<NonRegisteredUser>
{
    public void Configure(EntityTypeBuilder<NonRegisteredUser> builder)
    {
        builder
            .HasOne(nru => nru.PaymentProfile)
            .WithOne(nru => nru.NonRegisteredUser)
            .HasForeignKey<UserPaymentProfile>(upp => upp.NonRegisteredUserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
