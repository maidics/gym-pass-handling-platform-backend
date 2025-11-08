using FitPass.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FitPass.Infrastructure.Data.Configurations;

public class GymPassUsageConfiguration : IEntityTypeConfiguration<GymPassUsage>
{
    public void Configure(EntityTypeBuilder<GymPassUsage> builder)
    {
        builder
            .HasOne(gpu => gpu.Pass)
            .WithMany()
            .HasForeignKey(gpu => gpu.PassId);
    }
}
