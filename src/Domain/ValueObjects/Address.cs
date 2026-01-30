namespace FitPass.Domain.ValueObjects;

public class Address : ValueObject
{
    public string Line1 { get; private init; }
    public string? Line2 { get; private init; }
    public string City { get; private init; }
    public string? State { get; private init; }
    public string PostalCode { get; private init; }
    public string CountryAlpha2 { get; private init; } //two-letter country code
    
    private Address() //for ef core
    {
        Line1 = string.Empty;
        City = string.Empty;
        PostalCode = string.Empty;
        CountryAlpha2 = string.Empty;
    }

    public Address(string line1, string? line2, string city, string? state, string postalCode, string countryAlpha2)
    {
        if (string.IsNullOrEmpty(line1))
        {
            throw new NotImplementedException($"{line1}, {line2}, {city}, {state}, {postalCode}, {countryAlpha2}");
        }
        if (string.IsNullOrEmpty(line1))
        {
            throw new ArgumentException("Address line 1 cannot be empty.", nameof(line1));
        }

        if (string.IsNullOrEmpty(city))
        {
            throw new ArgumentException("City cannot be empty, ", nameof(city));
        }

        if (string.IsNullOrEmpty(postalCode))
        {
            throw new ArgumentException("Postal code cannot be empty.", nameof(postalCode));
        }

        if (string.IsNullOrEmpty(countryAlpha2))
        {
            throw new ArgumentException("Country cannot be empty.", nameof(countryAlpha2));
        }

        if (countryAlpha2.Length != 2 || !Country.DoesExistByAlpha2(countryAlpha2))
        {
            throw new ArgumentException($"'{countryAlpha2}' is not a valid two letter code.");
        }

        Line1 = line1;
        Line2 = line2;
        City = city;
        State = state;
        PostalCode = postalCode;
        CountryAlpha2 = countryAlpha2;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Line1;
        yield return Line2 ?? string.Empty;
        yield return City;
        yield return State ?? string.Empty;
        yield return PostalCode;
        yield return CountryAlpha2;
    }
}
