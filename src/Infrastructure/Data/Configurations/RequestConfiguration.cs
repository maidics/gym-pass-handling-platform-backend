using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FitPass.Infrastructure.Data.Configurations;

public class RequestConfiguration : IEntityTypeConfiguration<Request>
{
    public void Configure(EntityTypeBuilder<Request> builder)
    {
        builder.Property(r => r.Title).HasMaxLength(MaxStringLengths.Title);

        builder.Property(r => r.Description).HasMaxLength(MaxStringLengths.Description);

        builder.HasOne<ApplicationUser>().WithMany(au => au.Requests).HasForeignKey(r => r.CreatedBy);
    }
}
