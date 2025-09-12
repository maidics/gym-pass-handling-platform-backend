using FitPass.Domain.Entities;
using FitPass.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FitPass.Infrastructure.Data.Configurations;

public class GymStaffAssignmentConfiguration : IEntityTypeConfiguration<GymStaffAssigment>
{
    public void Configure(EntityTypeBuilder<GymStaffAssigment> builder)
    {
        builder.HasKey(gsa => gsa.ApplicationUserId);

        builder.HasOne<ApplicationUser>().WithOne(au => au.GymStaffAssigment);
    }
}