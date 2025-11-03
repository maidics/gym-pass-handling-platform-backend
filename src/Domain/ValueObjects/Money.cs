
using System.Globalization;

namespace FitPass.Domain.ValueObjects;

public class Money : ValueObject
{
    public decimal Amount { get; }
    public string Currency { get; }

    public static Money Zero(string currency) => new(0, currency);
    public static Money Usd(decimal amount) => new(amount, "USD");
    public static Money Eur(decimal amount) => new(amount, "EUR");

    private Money()
    {
        Currency = string.Empty;
    }

    public Money(decimal amount, string currency)
    {
        if (amount < 0)
        {
            throw new ArgumentException("Amount cannot be negative", nameof(amount));
        }

        amount = Math.Round(amount, 2);

        if (string.IsNullOrWhiteSpace(currency) || string.IsNullOrEmpty(currency))
        {
            throw new ArgumentException("Currency code is required.", nameof(currency));
        }

        currency = currency.Trim().ToUpperInvariant();

        if (currency.Length != 3)
        {
            throw new ArgumentException("Currency code must be a 3-letter ISO code.", nameof(currency));
        }

        if (!IsValidCurrencyCode(currency))
        {
            throw new InvalidCurrencyException($"'{currency}' is not a valid ISO currency code.");
        }

        Amount = amount;
        Currency = currency;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }

    public Money Add(Money other)
    {
        if (other is null)
        {
            throw new ArgumentNullException(nameof(other));
        }

        if (Currency != other.Currency)
        {
            throw new InvalidOperationException($"Cannot add money in different currencies: {Currency} and {other.Currency}");
        }

        return new Money(Amount + other.Amount, Currency);
    }

    public Money Subtract(Money other)
    {
        if (other is null)
        {
            throw new ArgumentNullException(nameof(other));
        }

        if (Currency != other.Currency)
        {
            throw new InvalidOperationException($"Cannot subtract money in different currencies: {Currency} and {other.Currency}");
        }

        var result = Amount - other.Amount;

        if (result < 0)
        {
            throw new InvalidOperationException($"Cannot subtract {other} from {this}: would result in negative amount.");
        }

        return new Money(result, Currency);
    }

    public Money Multiply(decimal factor)
    {
        if (factor < 0)
        {
            throw new ArgumentException("Multiplictaion factor cannot be negative.", nameof(factor));
        }

        return new Money(Amount * factor, Currency);
    }

    public Money Divide(decimal divisor)
    {
        if (divisor == 0)
        {
            throw new DivideByZeroException("Cannot divide money by zero.");
        }

        if (divisor < 0)
        {
            throw new ArgumentException("Divisor cannot be negative", nameof(divisor));
        }

        return new Money(Amount / divisor, Currency);
    }

    public Money ApplyDiscount(decimal discountPercentage)
    {
        if (discountPercentage < 0 || discountPercentage > 1)
        {
            throw new ArgumentOutOfRangeException(nameof(discountPercentage), "Discount percentage must be between 0 and 1");
        }

        var discountedAmount = Amount * (1 - discountPercentage);

        return new Money(discountedAmount, Currency);
    }

    public Money ApplyTax(decimal taxPercentage)
    {
        if (taxPercentage < 0)
        {
            throw new ArgumentException("Tax percentage cannot be negative.", nameof(taxPercentage));
        }

        var taxedAmount = Amount * (1 + taxPercentage);

        return new Money(taxedAmount, Currency);
    }

    public static Money operator +(Money left, Money right)
    {
        if (left is null)
        {
            throw new ArgumentNullException(nameof(left));
        }

        return left.Add(right);
    }

    public static Money operator -(Money left, Money right)
    {
        if (left is null)
        {
            throw new ArgumentNullException(nameof(left));
        }

        return left.Subtract(right);
    }

    public static Money operator *(Money money, decimal factor)
    {
        if (money is null)
        {
            throw new ArgumentNullException(nameof(money));
        }

        return money.Multiply(factor);
    }

    public static Money operator /(Money money, decimal divisor)
    {
        if (money is null)
        {
            throw new ArgumentNullException(nameof(money));
        }

        return money.Divide(divisor);
    }

    public int CompareTo(Money? other)
    {
        if (other is null) return 1;

        if (Currency != other.Currency)
        {
            throw new InvalidOperationException($"Cannot compare money in different currencies: {Currency} and {other.Currency}.");
        }

        return Amount.CompareTo(other.Amount);
    }

    public static bool operator <(Money left, Money right)
    {
        if (left is null)
        {
            throw new ArgumentNullException(nameof(left));
        }

        return left.CompareTo(right) < 0;
    }

    public static bool operator <=(Money left, Money right)
    {
        if (left is null)
        {
            throw new ArgumentNullException(nameof(left));
        }

        return left.CompareTo(right) <= 0;
    }

    public static bool operator >(Money left, Money right)
    {
        if (left is null)
        {
            throw new ArgumentNullException(nameof(left));
        }

        return left.CompareTo(right) > 0;
    }

    public static bool operator >=(Money left, Money right)
    {
        if (left is null)
        {
            throw new ArgumentNullException(nameof(left));
        }

        return left.CompareTo(right) >= 0;
    }

    public bool IsZero() => Amount == 0;

    public bool HasSameCurrency(Money other)
    {
        if (other is null) return false;
        return Currency == other.Currency;
    }

    public override string ToString()
    {
        var cultureInfo = GetCultureInfoForCurrency(Currency);
        return $"{Amount.ToString("C", cultureInfo)} {Currency}";
    }
    
    public string ToStringWithoutSymbol()
    {
        return $"{Amount:F2} {Currency}";
    }

    public void Deconstruct(out decimal amount, out string currency)
    {
        amount = Amount;
        currency = Currency;
    }

    private static bool IsValidCurrencyCode(string code)
    {
        var validCodes = new HashSet<string>
        {
            "USD", "EUR", "GBP", "JPY", "CHF", "CAD", "AUD", "NZD",
            "SEK", "NOK", "DKK", "PLN", "CZK", "HUF", "RON", "BGN",
            "HRK", "RUB", "TRY", "CNY", "INR", "KRW", "SGD", "HKD",
            "MXN", "ZAR", "BRL", "ARS", "CLP", "COP", "PEN", "UYU"
        };

        return validCodes.Contains(code);
    }

    private static CultureInfo GetCultureInfoForCurrency(string currency)
    {
        return currency switch
        {
            "USD" => new CultureInfo("en-US"),
            "EUR" => new CultureInfo("de-DE"),
            "GBP" => new CultureInfo("en-GB"),
            "JPY" => new CultureInfo("ja-JP"),
            "CAD" => new CultureInfo("en-CA"),
            "AUD" => new CultureInfo("en-AU"),
            _ => CultureInfo.InvariantCulture
        };
    }
}
