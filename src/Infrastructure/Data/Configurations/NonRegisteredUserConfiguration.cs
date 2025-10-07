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
            .WithOne()
            .HasForeignKey<NonRegisteredUser>(nru => nru.UserPaymentProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(nru => nru.PaymentProfile)
            .WithOne()
            .HasForeignKey<UserPaymentProfile>(upp => upp.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}