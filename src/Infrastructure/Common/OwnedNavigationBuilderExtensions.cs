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
            navigation.Property(x => x.Line1).HasMaxLength(MaxLengths.AddressLine1);
            
            navigation.Property(x => x.Line2).HasMaxLength(MaxLengths.AddressLine2);
            
            navigation.Property(x => x.City).HasMaxLength(MaxLengths.City);
            
            navigation.Property(x => x.State).HasMaxLength(MaxLengths.State);
            
            navigation.Property(x => x.PostalCode).HasMaxLength(MaxLengths.PostalCode);
            
            navigation.Property(x => x.CountryAlpha2).HasMaxLength(MaxLengths.CountryAlpha2);

            return navigation;
        }
    }

    extension<TOwnerEntity>(OwnedNavigationBuilder<TOwnerEntity, PhoneNumber> navigation)
        where TOwnerEntity : BaseEntity
    {
        public OwnedNavigationBuilder<TOwnerEntity, PhoneNumber> ConfigurePhoneNumber()
        {
            navigation.Property(x => x.Value).HasMaxLength(MaxLengths.PhoneNumber);

            return navigation;
        }
    }

    extension<TOwnerEntity>(OwnedNavigationBuilder<TOwnerEntity, Money> navigation) where TOwnerEntity : BaseEntity
    {
        public OwnedNavigationBuilder<TOwnerEntity, Money> ConfigureMoney()
        {
            navigation.Property(x => x.Amount).HasPrecision(18, 2);
            
            navigation.Property(x => x.Currency).HasMaxLength(3); //3 letter ISO code

            return navigation;
        }
    }
}
