using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FitPass.Infrastructure.Data.Configurations;

public class GymStaffAssignmentConfiguration : IEntityTypeConfiguration<GymEmployment>
{
    public void Configure(EntityTypeBuilder<GymEmployment> builder)
    {
        builder.HasKey(gsa => gsa.ApplicationUserId);

        builder
            .HasOne(gsa => gsa.ApplicationUser)
            .WithOne(au => au.GymStaffAssignment);

        builder.Property(gsa => gsa.EscalationEmail).HasMaxLength(MaxStringLengths.Email);
    }
}
