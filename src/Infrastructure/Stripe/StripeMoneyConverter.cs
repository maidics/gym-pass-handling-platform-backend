using FitPass.Domain.ValueObjects;

namespace FitPass.Infrastructure.Stripe;

public static class MoneyExtensions
{
    public static long ToStripeAmount(this Money money)
    {
        if (money == null)
        {
            throw new ArgumentNullException(nameof(money), "Money cannot be null.");
        }

        return (long)Math.Round(money.Amount * 100, MidpointRounding.AwayFromZero);
    }

    public static Money FromStripeAmount(long stripeAmount, string currency)
    {
        decimal amount = stripeAmount / 100m;

        return new Money(amount, currency);
    }

    public static string ToStripeCurrency(this Money money)
    {
        return money.Currency.ToLowerInvariant();
    }

    public static void ValidateForStripe(Money money)
    {
        var currency = money.Currency.ToUpperInvariant();
        var stripeAmount = ToStripeAmount(money);

        var minimumAmounts = new Dictionary<string, long>
        {
            { "USD", 50 },    // $0.50
            { "EUR", 50 },    // €0.50
            { "HUF", 175 },   // 175 Ft
        };

        if (!minimumAmounts.TryGetValue(currency, out var minimum))
        {
            throw new ArgumentException(
               $"Currency '{currency}' is not currently supported for payment processing.",
               nameof(money));
        }

        if (stripeAmount < minimum)
        {
            var minimumMoney = FromStripeAmount(minimum, currency);
            throw new ArgumentException(
                $"Amount {money} is below Stripe's minimum charge amount of {minimumMoney} for {currency}",
                nameof(money));
        }
    }
}
