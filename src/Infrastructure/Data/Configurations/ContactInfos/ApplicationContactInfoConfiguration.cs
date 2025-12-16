using FitPass.Domain.Constants;
using FitPass.Domain.Entities.ContactInfos;
using FitPass.Infrastructure.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FitPass.Infrastructure.Data.Configurations.ContactInfos;

public class ApplicationContactInfoConfiguration : IEntityTypeConfiguration<ApplicationContactInfo>
{
    public void Configure(EntityTypeBuilder<ApplicationContactInfo> builder)
    {
        builder.Property(x => x.Email).HasMaxLength(MaxLength.Email);

        builder.OwnsOne(x => x.Address).ConfigureAddress();
        
        builder.OwnsOne(x => x.PhoneNumber).ConfigurePhoneNumber();
    }
}
