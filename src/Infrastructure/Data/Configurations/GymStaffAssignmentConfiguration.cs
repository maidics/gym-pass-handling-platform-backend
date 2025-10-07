using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FitPass.Infrastructure.Data.Configurations;

public class GymStaffAssignmentConfiguration : IEntityTypeConfiguration<GymStaffAssigment>
{
    public void Configure(EntityTypeBuilder<GymStaffAssigment> builder)
    {
        builder.HasKey(gsa => gsa.ApplicationUserId);

        builder
            .HasOne(gsa => gsa.ApplicationUser)
            .WithOne(au => au.GymStaffAssigment)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Property(gsa => gsa.EscalationEmail).HasMaxLength(MaxStringLengths.Email);
    }
}