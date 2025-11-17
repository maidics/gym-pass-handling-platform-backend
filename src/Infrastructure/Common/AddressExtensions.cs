using Stripe;

namespace FitPass.Infrastructure.Common;

public static class AddressExtensions
{
    public static Domain.ValueObjects.Address FromStripeAddress(this Address stripeAddress)
    {
        return new Domain.ValueObjects.Address(
            stripeAddress.Line1, 
            stripeAddress.Line2, 
            stripeAddress.City,
            stripeAddress.State,
            stripeAddress.PostalCode,
            stripeAddress.Country);
    }

    public static AddressOptions ToStripeAddressOptions(this Domain.ValueObjects.Address address)
    {
        return new AddressOptions
        {
            Line1 = address.Line1,
            Line2 = address.Line2,
            City = address.City,
            State = address.State,
            PostalCode = address.PostalCode,
            Country = address.CountryAlpha2
        };
    }
}
