using FitPass.Domain.Exceptions;
using FitPass.Domain.ValueObjects;
using NUnit.Framework;
using Shouldly;

namespace FitPass.Domain.UnitTests.ValueObjects;

public class MoneyTests
{
    [TestCase(100, "USD", 100, "usd")]
    [TestCase(100.50, "EUR", 100.50, "eur")]
    [TestCase(50, "  gbp  ", 50, "gbp")]
    [TestCase(0, "usd", 0, "usd")]
    public void ConstructorShouldReturnMoney(decimal inputAmount, string inputCurrency, decimal expectedAmount, string expectedCurrency)
    {
        var money = new Money(inputAmount, inputCurrency);

        money.ShouldSatisfyAllConditions(
            () => money.Amount.ShouldBe(expectedAmount),
            () => money.Currency.ShouldBe(expectedCurrency)
        );
    }

    [TestCase(10.123, "usd", 10.12)]
    [TestCase(10.125, "usd", 10.13)]
    [TestCase(100.1, "jpy", 100)]
    [TestCase(100.5, "jpy", 101)]
    [TestCase(50.99, "huf", 50.99)]
    public void ShouldRoundAmount(decimal inputAmount, string currency, decimal expectedAmount)
    {
        var money = new Money(inputAmount, currency);
        money.Amount.ShouldBe(expectedAmount);
    }

    [TestCase(-1, "usd")]
    [TestCase(-0.01, "eur")]
    public void ShouldThrowWhenAmountIsNegative(decimal amount, string currency)
    {
        Should.Throw<ArgumentException>(() => new Money(amount, currency))
            .Message.ShouldContain("Amount cannot be negative");
    }

    [TestCase("")]
    [TestCase("   ")]
    public void ShouldThrowWhenCurrencyIsEmpty(string currency)
    {
        Should.Throw<ArgumentException>(() => new Money(100, currency))
            .Message.ShouldContain("Currency code is required");
    }

    [TestCase("us")]
    [TestCase("usdd")]
    public void ShouldThrowWhenCurrencyLengthInvalid(string currency)
    {
        Should.Throw<ArgumentException>(() => new Money(100, currency))
            .Message.ShouldContain("must be a 3-letter ISO code");
    }

    [TestCase("zzz")]
    [TestCase("xxy")]
    public void ShouldThrowWhenCurrencyUnknown(string currency)
    {
        Should.Throw<InvalidCurrencyException>(() => new Money(100, currency))
            .Message.ShouldContain("is not a valid ISO currency code");
    }

    [Test]
    public void ShouldCreateUsdMoney()
    {
        var money = Money.Usd(50);
        money.Amount.ShouldBe(50);
        money.Currency.ShouldBe("usd");
        money.IsZero().ShouldBeFalse();
    }

    [Test]
    public void ShouldCreateEurMoney()
    {
        var money = Money.Eur(25);
        money.Amount.ShouldBe(25);
        money.Currency.ShouldBe("eur");
    }

    [Test]
    public void ShouldSumAmountsWhenCurrenciesMatch()
    {
        var m1 = Money.Usd(10);
        var m2 = Money.Usd(20);

        var result = m1.Add(m2);

        result.Amount.ShouldBe(30);
        result.Currency.ShouldBe("usd");
    }

    [Test]
    public void OperatorPlusShouldSumAmounts()
    {
        var result = Money.Usd(10) + Money.Usd(20);
        result.Amount.ShouldBe(30);
    }

    [Test]
    public void ShouldThrowInvalidOperationWhenCurrenciesMismatch()
    {
        var m1 = Money.Usd(10);
        var m2 = Money.Eur(10);

        Should.Throw<InvalidOperationException>(() => m1.Add(m2))
            .Message.ShouldContain("Cannot add money in different currencies");
    }

    [Test]
    public void SubtractShouldDeductAmountsWhenCurrenciesMatchAndResultPositive()
    {
        var m1 = Money.Usd(50);
        var m2 = Money.Usd(20);

        var result = m1.Subtract(m2);

        result.Amount.ShouldBe(30);
    }

    [Test]
    public void ShouldThrowInvalidOperationWhenResultNegative()
    {
        var m1 = Money.Usd(10);
        var m2 = Money.Usd(20);

        Should.Throw<InvalidOperationException>(() => m1.Subtract(m2))
            .Message.ShouldContain("would result in negative amount");
    }

    [Test]
    public void ShouldScaleAmount()
    {
        var money = Money.Usd(10);
        var result = money.Multiply(2.5m);

        result.Amount.ShouldBe(25);
    }

    [Test]
    public void ShouldThrowArgumentWhenFactorNegative()
    {
        Should.Throw<ArgumentException>(() => Money.Usd(10).Multiply(-1));
    }

    [Test]
    public void ShouldScaleDownAmount()
    {
        var money = Money.Usd(10);
        var result = money.Divide(2);

        result.Amount.ShouldBe(5);
    }

    [Test]
    public void ShouldThrowWhenDivisorIsZero()
    {
        Should.Throw<DivideByZeroException>(() => Money.Usd(10).Divide(0));
    }

    [Test]
    public void ApplyDiscountShouldReduceAmount()
    {
        var money = Money.Usd(100);
        var result = money.ApplyDiscount(0.2m);

        result.Amount.ShouldBe(80);
    }

    [TestCase(-0.1)]
    [TestCase(1.1)]
    public void ApplyDiscountShouldThrowWhenPercentageInvalid(decimal discount)
    {
        Should.Throw<ArgumentOutOfRangeException>(() => Money.Usd(100).ApplyDiscount(discount));
    }

    [Test]
    public void ApplyTaxShouldIncreaseAmount()
    {
        var money = Money.Usd(100);
        var result = money.ApplyTax(0.1m);

        result.Amount.ShouldBe(110);
    }

    [Test]
    public void EqualsShouldReturnTrueForSameAmountsAndCurrency()
    {
        var m1 = new Money(10, "usd");
        var m2 = new Money(10, "USD");

        m1.ShouldBe(m2);
        (m1 == m2).ShouldBeTrue();
    }

    [Test]
    public void EqualsShouldReturnFalseForDifferentAmounts()
    {
        var m1 = Money.Usd(10);
        var m2 = Money.Usd(11);

        m1.ShouldNotBe(m2);
    }

    [Test]
    public void CompareToShouldWorkCorrectly()
    {
        var small = Money.Usd(10);
        var large = Money.Usd(20);

        (small < large).ShouldBeTrue();
        (large > small).ShouldBeTrue();
        (small <= large).ShouldBeTrue();
        #pragma warning disable CS1718
        (small >= small).ShouldBeTrue();
        #pragma warning restore CS1718
    }

    [Test]
    public void CompareToShouldThrowWhenCurrenciesMismatch()
    {
        var usd = Money.Usd(10);
        var eur = Money.Eur(10);

        Should.Throw<InvalidOperationException>(() => usd.CompareTo(eur));
    }

    [Test]
    public void HasSameCurrencyShouldReturnTrueWhenCurrenciesMatch()
    {
        var m1 = Money.Usd(10);
        var m2 = Money.Usd(100);

        m1.HasSameCurrency(m2).ShouldBeTrue();
    }

    [Test]
    public void ToStringShouldFormatBasedOnInternalCultureInfo()
    {

        var usd = Money.Usd(1234.56m);
        usd.ToString().ShouldBe("1,234.56 USD");

        var eur = Money.Eur(1234.56m);
        eur.ToString().ShouldBe("1.234,56 EUR");

        var jpy = new Money(1000, "jpy");
        jpy.ToString().ShouldContain("1,000 JPY");
    }

    [Test]
    public void ToStringWithoutSymbolShouldReturnSimpleFormat()
    {
        var money = Money.Usd(10.5m);
        money.ToStringWithoutSymbol().ShouldBe("10.50 usd");
    }
}
