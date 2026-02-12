using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using FitPass.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FitPass.Infrastructure.Data.Configurations;

public class RequestConfiguration : IEntityTypeConfiguration<Request>
{
    public void Configure(EntityTypeBuilder<Request> builder)
    {
        builder.Property(r => r.Title).HasMaxLength(MaxLengths.Title);

        builder.Property(r => r.Description).HasMaxLength(MaxLengths.Description);

        builder.HasOne<ApplicationUser>().WithMany().HasForeignKey(r => r.CreatedBy);
    }
}
