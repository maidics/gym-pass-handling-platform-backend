using FitPass.Domain.Common;
using FitPass.Domain.Constants;
using FitPass.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FitPass.Infrastructure.Common;

public static class OwnedNavigationBuilderExtensions
{
    extension<TOwnerEntity>(OwnedNavigationBuilder<TOwnerEntity, Address> navigation) 
        where TOwnerEntity : BaseEntity
    {
        public OwnedNavigationBuilder<TOwnerEntity, Address> ConfigureAddress()
        {
            navigation.Property(x => x.Line1).HasMaxLength(MaxLength.AddressLine1);
            
            navigation.Property(x => x.Line2).HasMaxLength(MaxLength.AddressLine2);
            
            navigation.Property(x => x.City).HasMaxLength(MaxLength.City);
            
            navigation.Property(x => x.State).HasMaxLength(MaxLength.State);
            
            navigation.Property(x => x.PostalCode).HasMaxLength(MaxLength.PostalCode);
            
            navigation.Property(x => x.CountryAlpha2).HasMaxLength(MaxLength.CountryAlpha2);

            return navigation;
        }
    }

    extension<TOwnerEntity>(OwnedNavigationBuilder<TOwnerEntity, PhoneNumber> navigation)
        where TOwnerEntity : BaseEntity
    {
        public OwnedNavigationBuilder<TOwnerEntity, PhoneNumber> ConfigurePhoneNumber()
        {
            navigation.Property(x => x.Value).HasMaxLength(MaxLength.PhoneNumber);

            return navigation;
        }
    }
}
