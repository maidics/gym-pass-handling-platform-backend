
using System.Collections.Frozen;
using System.Globalization;

namespace FitPass.Domain.ValueObjects;

public class Money : ValueObject
{
    public decimal Amount { get; private set; } //private set so ef core does not ignore these properties
    public string Currency { get; private set; }

    public static Money Usd(decimal amount) => new(amount, "usd");
    public static Money Eur(decimal amount) => new(amount, "eur");

    private Money()
    {
        Currency = string.Empty;
    }

    public Money(decimal amount, string currency)
    {
        currency = currency.Trim().ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(currency) || string.IsNullOrEmpty(currency))
        {
            throw new ArgumentException("Currency code is required.", nameof(currency));
        }

        if (currency.Length != 3)
        {
            throw new ArgumentException("Currency code must be a 3-letter ISO code.", nameof(currency));
        }

        if (!IsValidCurrency(currency))
        {
            throw new InvalidCurrencyException($"'{currency}' is not a valid ISO currency code.");
        }

        if (amount < 0)
        {
            throw new ArgumentException("Amount cannot be negative", nameof(amount));
        }

        if (IsZeroDecimal(currency))
        {
            amount = Math.Round(amount, 0, MidpointRounding.AwayFromZero);
        } else
        {
            amount = Math.Round(amount, 2, MidpointRounding.AwayFromZero);
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

        string format = IsZeroDecimal(Currency) ? "N0" : "N2";

        return $"{Amount.ToString(format, cultureInfo)} {Currency.ToUpperInvariant()}";
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

    private static bool IsValidCurrency(string code)
    {
        return Currencies.Contains(code);
    }

    public static readonly FrozenSet<string> Currencies = [
        // North America
        "usd", "cad", "mxn", 

        // Europe
        "eur", "gbp", "chf", "sek", "nok", "dkk", "pln", "czk", "huf", "ron", 
        "bgn", "all", "amd", "bam", "gel", "gip", "mdl", "mkd", "rsd", "uah", 

        // Asia / Pacific
        "aud", "nzd", "jpy", "cny", "hkd", "sgd", "inr", "idr", "krw", "myr", 
        "php", "thb", "vnd", "pkr", "bdt", "lkr", "mvr", "npr", "aed", 
        "ils", "sar", "qar", "lbp", "afn", "azn", 
        "bnd", "khr", "kgs", "kzt", "lak", "mnt", "mmk", "pgk", "tjs", "top", 
        "uzs", "vuv", "wst", "yer", 

        // Latin America & Caribbean
        "brl", "ars", "clp", "cop", "pen", "uyu", "bob", "crc", "dop", "gtq", 
        "hnl", "nio", "pab", "pyg", "ang", "awg", "bbd", "bmd", "bsd", "bzd", 
        "fjd", "gyd", "htg", "jmd", "kyd", "srd", "ttd", "xcd", 

        // Africa
        "zar", "egp", "ngn", "kes", "mad", "tzs", "ugx", "aoa", "bif", "bwp", 
        "cdf", "cve", "djf", "dzd", "etb", "gmd", "gnf", "lsl", "lrd", "mga", 
        "mro", "mur", "mwk", "mzn", "nad", "rwf", "scr", "sll", "sos", "std", 
        "szl", "xaf", "xof", "zmw",

        // Others / Special
        "try", "rub", "xpf"
    ];

    public static bool IsZeroDecimal(string currency)
    {
        return ZeroDecimalCurrencies.Contains(currency.ToLowerInvariant());
    }

    public static readonly FrozenSet<string> ZeroDecimalCurrencies = [
        "bif", "clp", "djf", "gnf", "jpy", "krw", 
        "mga", "pyg", "rwf", "ugx", "vnd", "vuv", "xaf", "xof", "xpf" 
    ];

    private static CultureInfo GetCultureInfoForCurrency(string currency)
    {
        return currency switch
        {
            "usd" => new CultureInfo("en-US"),
            "eur" => new CultureInfo("de-DE"),
            "gbp" => new CultureInfo("en-GB"),
            "jpy" => new CultureInfo("ja-JP"),
            "cad" => new CultureInfo("en-CA"),
            "aud" => new CultureInfo("en-AU"),
            _ => CultureInfo.InvariantCulture
        };
    }
}
