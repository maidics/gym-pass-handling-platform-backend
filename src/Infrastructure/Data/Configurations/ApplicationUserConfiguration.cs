using FitPass.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FitPass.Infrastructure.Data.Configurations;

public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.HasMany(au => au.UserGymMemberships).WithOne(ugm => ugm.ApplicationUser).HasForeignKey(ugm => ugm.ApplicationUserId);

        builder.HasOne(au => au.GymStaffAssigment).WithOne(gsa => gsa.ApplicationUser).HasForeignKey<GymStaffAssigment>(gsa => gsa.ApplicationUserId);
    }
}