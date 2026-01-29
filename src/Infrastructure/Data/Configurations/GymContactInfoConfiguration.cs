using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FitPass.Infrastructure.Common;

namespace FitPass.Infrastructure.Data.Configurations.ContactInfos;

public class GymContactInfoConfiguration : IEntityTypeConfiguration<GymContactInfo>
{
    public void Configure(EntityTypeBuilder<GymContactInfo> builder)
    {
        builder.Property(x => x.FullName).HasMaxLength(MaxLengths.FullName);

        builder.OwnsOne(x => x.Address).ConfigureAddress();

        builder.OwnsOne(x => x.PhoneNumber).ConfigurePhoneNumber();
    }
}
