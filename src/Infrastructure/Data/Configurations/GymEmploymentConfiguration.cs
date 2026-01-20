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
            .HasForeignKey<GymEmployment>(ge => ge.UserId);

        builder
            .HasOne(ge => ge.Gym)
            .WithMany()
            .HasForeignKey(ge => ge.GymId);

        builder.Property(gsa => gsa.SupervisorEmail).HasMaxLength(MaxLengths.Email);
    }
}
