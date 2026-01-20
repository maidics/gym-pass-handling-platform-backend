
using System.Collections.Frozen;
using System.Globalization;

namespace FitPass.Domain.ValueObjects;

public class Money : ValueObject
{
    public decimal Amount { get; private set; } //private set so ef core does not ignore these properties
    public CurrencyCode Currency { get; private set; }

    private Money() { }

    public Money(decimal amount, CurrencyCode currency)
    {
        if (amount < 0)
        {
            throw new ArgumentException("Amount cannot be negative", nameof(amount));
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

        //string format = IsZeroDecimal(Currency) ? "N0" : "N2";

        return $"{Amount.ToString("N", cultureInfo)} {Currency}";
    }

    public void Deconstruct(out decimal amount, out CurrencyCode currency)
    {
        amount = Amount;
        currency = Currency;
    }

    /*
    public static bool IsZeroDecimal(string currency)
    {
        return ZeroDecimalCurrencies.Contains(currency.ToLowerInvariant());
    }

    public static readonly FrozenSet<string> ZeroDecimalCurrencies = [
        "bif", "clp", "djf", "gnf", "jpy", "krw", 
        "mga", "pyg", "rwf", "ugx", "vnd", "vuv", "xaf", "xof", "xpf" 
    ];
    */

    private static CultureInfo GetCultureInfoForCurrency(CurrencyCode currency)
    {
        return currency switch
        {
            CurrencyCode.USD => new CultureInfo("en-US"),
            CurrencyCode.EUR => new CultureInfo("de-DE"),
            CurrencyCode.HUF => new CultureInfo("hu-HU"),
            _ => CultureInfo.InvariantCulture
        };
    }
}
