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
    public void ShouldThrowIfAmountIsNegative(decimal amount, string currency)
    {
        Should.Throw<ArgumentException>(() => new Money(amount, currency))
            .Message.ShouldContain("Amount cannot be negative");
    }

    [TestCase("")]
    [TestCase("   ")]
    public void ShouldThrowIfCurrencyIsEmpty(string currency)
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
    public void ShouldThrowIfCurrencyUnknown(string currency)
    {
        Should.Throw<InvalidCurrencyException>(() => new Money(100, currency))
            .Message.ShouldContain("is not a valid ISO currency code");
    }

    [Test]
    public void Zero_ShouldCreateZeroAmount()
    {
        var money = Money.Zero("usd");
        money.Amount.ShouldBe(0);
        money.Currency.ShouldBe("usd");
        money.IsZero().ShouldBeTrue();
    }

    [Test]
    public void Usd_ShouldCreateUsdMoney()
    {
        var money = Money.Usd(50);
        money.Amount.ShouldBe(50);
        money.Currency.ShouldBe("usd");
    }

    [Test]
    public void Eur_ShouldCreateEurMoney()
    {
        var money = Money.Eur(25);
        money.Amount.ShouldBe(25);
        money.Currency.ShouldBe("eur");
    }

    [Test]
    public void Add_ShouldSumAmounts_WhenCurrenciesMatch()
    {
        var m1 = Money.Usd(10);
        var m2 = Money.Usd(20);

        var result = m1.Add(m2);

        result.Amount.ShouldBe(30);
        result.Currency.ShouldBe("usd");
    }

    [Test]
    public void OperatorPlus_ShouldSumAmounts()
    {
        var result = Money.Usd(10) + Money.Usd(20);
        result.Amount.ShouldBe(30);
    }

    [Test]
    public void Add_ShouldThrowInvalidOperation_WhenCurrenciesMismatch()
    {
        var m1 = Money.Usd(10);
        var m2 = Money.Eur(10);

        Should.Throw<InvalidOperationException>(() => m1.Add(m2))
            .Message.ShouldContain("Cannot add money in different currencies");
    }

    [Test]
    public void Subtract_ShouldDeductAmounts_WhenCurrenciesMatchAndResultPositive()
    {
        var m1 = Money.Usd(50);
        var m2 = Money.Usd(20);

        var result = m1.Subtract(m2);

        result.Amount.ShouldBe(30);
    }

    [Test]
    public void Subtract_ShouldThrowInvalidOperation_WhenResultNegative()
    {
        var m1 = Money.Usd(10);
        var m2 = Money.Usd(20);

        Should.Throw<InvalidOperationException>(() => m1.Subtract(m2))
            .Message.ShouldContain("would result in negative amount");
    }

    [Test]
    public void Multiply_ShouldScaleAmount()
    {
        var money = Money.Usd(10);
        var result = money.Multiply(2.5m);

        result.Amount.ShouldBe(25);
    }

    [Test]
    public void Multiply_ShouldThrowArgument_WhenFactorNegative()
    {
        Should.Throw<ArgumentException>(() => Money.Usd(10).Multiply(-1));
    }

    [Test]
    public void Divide_ShouldScaleDownAmount()
    {
        var money = Money.Usd(10);
        var result = money.Divide(2);

        result.Amount.ShouldBe(5);
    }

    [Test]
    public void Divide_ShouldThrow_WhenDivisorIsZero()
    {
        Should.Throw<DivideByZeroException>(() => Money.Usd(10).Divide(0));
    }

    [Test]
    public void ApplyDiscount_ShouldReduceAmount()
    {
        var money = Money.Usd(100);
        var result = money.ApplyDiscount(0.2m);

        result.Amount.ShouldBe(80);
    }

    [TestCase(-0.1)]
    [TestCase(1.1)]
    public void ApplyDiscount_ShouldThrow_WhenPercentageInvalid(decimal discount)
    {
        Should.Throw<ArgumentOutOfRangeException>(() => Money.Usd(100).ApplyDiscount(discount));
    }

    [Test]
    public void ApplyTax_ShouldIncreaseAmount()
    {
        var money = Money.Usd(100);
        var result = money.ApplyTax(0.1m);

        result.Amount.ShouldBe(110);
    }

    [Test]
    public void Equals_ShouldReturnTrue_ForSameAmountsAndCurrency()
    {
        var m1 = new Money(10, "usd");
        var m2 = new Money(10, "USD");

        m1.ShouldBe(m2);
        (m1 == m2).ShouldBeTrue();
    }

    [Test]
    public void Equals_ShouldReturnFalse_ForDifferentAmounts()
    {
        var m1 = Money.Usd(10);
        var m2 = Money.Usd(11);

        m1.ShouldNotBe(m2);
    }

    [Test]
    public void CompareTo_ShouldWorkCorrectly()
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
    public void CompareTo_ShouldThrow_WhenCurrenciesMismatch()
    {
        var usd = Money.Usd(10);
        var eur = Money.Eur(10);

        Should.Throw<InvalidOperationException>(() => usd.CompareTo(eur));
    }

    [Test]
    public void HasSameCurrency_ShouldReturnTrue_WhenCurrenciesMatch()
    {
        var m1 = Money.Usd(10);
        var m2 = Money.Usd(100);

        m1.HasSameCurrency(m2).ShouldBeTrue();
    }

    [Test]
    public void ToString_ShouldFormatBasedOnInternalCultureInfo()
    {

        var usd = Money.Usd(1234.56m);
        usd.ToString().ShouldBe("1,234.56 USD");

        var eur = Money.Eur(1234.56m);
        eur.ToString().ShouldBe("1.234,56 EUR");

        var jpy = new Money(1000, "jpy");
        jpy.ToString().ShouldContain("1,000 JPY");
    }

    [Test]
    public void ToStringWithoutSymbol_ShouldReturnSimpleFormat()
    {
        var money = Money.Usd(10.5m);
        money.ToStringWithoutSymbol().ShouldBe("10.50 usd");
    }
}
