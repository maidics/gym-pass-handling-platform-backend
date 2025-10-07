using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FitPass.Infrastructure.Data.Configurations;

public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder
            .HasMany(au => au.UserGymMemberships)
            .WithOne(ugm => ugm.ApplicationUser)
            .HasForeignKey(ugm => ugm.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder
            .HasOne(au => au.GymStaffAssigment)
            .WithOne(gsa => gsa.ApplicationUser)
            .HasForeignKey<GymStaffAssigment>(gsa => gsa.ApplicationUserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(au => au.PaymentProfile)
            .WithOne()
            .HasForeignKey<UserPaymentProfile>(gsa => gsa.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(au => au.FirstName).HasMaxLength(MaxStringLengths.Name);

        builder.Property(au => au.LastName).HasMaxLength(MaxStringLengths.Name);
    }
}
