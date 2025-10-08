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
            .HasForeignKey(ugm => ugm.ApplicationUserId);

        builder
            .HasOne(au => au.GymStaffAssignment)
            .WithOne(gsa => gsa.ApplicationUser)
            .HasForeignKey<GymStaffAssignment>(gsa => gsa.ApplicationUserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(au => au.PaymentProfile)
            .WithOne(upp => upp.ApplicationUser)
            .HasForeignKey<UserPaymentProfile>(gsa => gsa.ApplicationUserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(au => au.FirstName).HasMaxLength(MaxStringLengths.Name);

        builder.Property(au => au.LastName).HasMaxLength(MaxStringLengths.Name);
    }
}
