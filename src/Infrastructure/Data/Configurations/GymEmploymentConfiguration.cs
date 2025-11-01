using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using FitPass.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FitPass.Infrastructure.Data.Configurations;

public class GymEmploymentConfiguration : IEntityTypeConfiguration<GymEmployment>
{
    public void Configure(EntityTypeBuilder<GymEmployment> builder)
    {
        builder
            .HasOne<ApplicationUser>()
            .WithOne()
            .HasForeignKey<GymEmployment>(ge => ge.ApplicationUserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder
            .HasOne(ge => ge.Gym)
            .WithOne()
            .HasForeignKey<GymEmployment>(ge => ge.GymId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Property(gsa => gsa.EscalationEmail).HasMaxLength(MaxStringLengths.Email);
    }
}
