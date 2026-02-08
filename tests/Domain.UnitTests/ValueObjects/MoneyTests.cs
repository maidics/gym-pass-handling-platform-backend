using FitPass.Domain.Enums;
using FitPass.Domain.ValueObjects;
using NUnit.Framework;
using Shouldly;

namespace FitPass.Domain.UnitTests.ValueObjects;

public class MoneyTests
{
    [TestCase(100, CurrencyCode.USD, 100, CurrencyCode.USD)]
    [TestCase(100.50, CurrencyCode.EUR, 100.50, CurrencyCode.EUR)]
    [TestCase(50, CurrencyCode.HUF, 50, CurrencyCode.HUF)]
    public void ConstructorShouldReturnMoney(
        decimal inputAmount,
        CurrencyCode inputCurrency,
        decimal expectedAmount,
        CurrencyCode expectedCurrency
    )
    {
        var money = new Money(inputAmount, inputCurrency);

        money.ShouldSatisfyAllConditions(
            () => money.Amount.ShouldBe(expectedAmount),
            () => money.Currency.ShouldBe(expectedCurrency)
        );
    }

    [TestCase(-1, CurrencyCode.EUR)]
    [TestCase(-0.01, CurrencyCode.EUR)]
    public void ShouldThrowWhenAmountIsNegative(decimal amount, CurrencyCode currency)
    {
        Should
            .Throw<ArgumentException>(() => new Money(amount, currency))
            .Message.ShouldContain("Amount cannot be negative");
    }

    [Test]
    public void ShouldSumAmountsWhenCurrenciesMatch()
    {
        var m1 = new Money(10, CurrencyCode.USD);
        var m2 = new Money(20, CurrencyCode.USD);

        var result = m1.Add(m2);

        result.Amount.ShouldBe(30);
        result.Currency.ShouldBe(CurrencyCode.USD);
    }

    [Test]
    public void OperatorPlusShouldSumAmounts()
    {
        var result = new Money(10, CurrencyCode.USD) + new Money(20, CurrencyCode.USD);
        result.Amount.ShouldBe(30);
    }

    [Test]
    public void ShouldThrowInvalidOperationWhenCurrenciesMismatch()
    {
        var m1 = new Money(10, CurrencyCode.USD);
        var m2 = new Money(10, CurrencyCode.EUR);

        Should
            .Throw<InvalidOperationException>(() => m1.Add(m2))
            .Message.ShouldContain("Cannot add money in different currencies");
    }

    [Test]
    public void SubtractShouldDeductAmountsWhenCurrenciesMatchAndResultIsPositiveAmount()
    {
        var m1 = new Money(50, CurrencyCode.USD);
        var m2 = new Money(20, CurrencyCode.USD);

        var result = m1.Subtract(m2);

        result.Amount.ShouldBe(30);
    }

    [Test]
    public void ShouldThrowInvalidOperationWhenResultWouldBeNegativeAmount()
    {
        var m1 = new Money(10, CurrencyCode.USD);
        var m2 = new Money(20, CurrencyCode.USD);

        Should
            .Throw<InvalidOperationException>(() => m1.Subtract(m2))
            .Message.ShouldContain("would result in negative amount");
    }

    [Test]
    public void ShouldScaleAmount()
    {
        var money = new Money(10, CurrencyCode.USD);
        var result = money.Multiply(2.5m);

        result.Amount.ShouldBe(25);
    }

    [Test]
    public void ShouldThrowArgumentWhenFactorForMultiplicationIsNegative()
    {
        Should.Throw<ArgumentException>(() => new Money(10, CurrencyCode.USD).Multiply(-1));
    }

    [Test]
    public void ShouldScaleDownAmount()
    {
        var money = new Money(10, CurrencyCode.USD);
        var result = money.Divide(2);

        result.Amount.ShouldBe(5);
    }

    [Test]
    public void ShouldThrowWhenDivisorIsZero()
    {
        Should.Throw<DivideByZeroException>(() => new Money(10, CurrencyCode.USD).Divide(0));
    }

    [Test]
    public void ApplyDiscountShouldReduceAmount()
    {
        var money = new Money(100, CurrencyCode.USD);
        var result = money.ApplyDiscount(0.2m);

        result.Amount.ShouldBe(80);
    }

    [TestCase(-0.1)]
    [TestCase(1.1)]
    public void ApplyDiscountShouldThrowWhenPercentageInvalid(decimal discount)
    {
        Should.Throw<ArgumentOutOfRangeException>(() =>
            new Money(100, CurrencyCode.USD).ApplyDiscount(discount)
        );
    }

    [Test]
    public void ApplyTaxShouldIncreaseAmount()
    {
        var money = new Money(100, CurrencyCode.USD);
        var result = money.ApplyTax(0.1m);

        result.Amount.ShouldBe(110);
    }

    [Test]
    public void EqualsShouldReturnTrueForSameAmountsAndCurrency()
    {
        var m1 = new Money(10, CurrencyCode.EUR);
        var m2 = new Money(10, CurrencyCode.EUR);

        m1.ShouldBe(m2);
        (m1 == m2).ShouldBeTrue();
    }

    [Test]
    public void EqualsShouldReturnFalseForDifferentAmounts()
    {
        var m1 = new Money(10, CurrencyCode.USD);
        var m2 = new Money(11, CurrencyCode.USD);

        m1.ShouldNotBe(m2);
    }

    [Test]
    public void CompareToShouldWorkCorrectly()
    {
        var small = new Money(10, CurrencyCode.USD);
        var large = new Money(20, CurrencyCode.USD);

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
        var usd = new Money(10, CurrencyCode.USD);
        var eur = new Money(10, CurrencyCode.EUR);

        Should.Throw<InvalidOperationException>(() => usd.CompareTo(eur));
    }

    [Test]
    public void HasSameCurrencyShouldReturnTrueWhenCurrenciesMatch()
    {
        var m1 = new Money(10, CurrencyCode.USD);
        var m2 = new Money(100, CurrencyCode.USD);

        m1.HasSameCurrency(m2).ShouldBeTrue();
    }
}
