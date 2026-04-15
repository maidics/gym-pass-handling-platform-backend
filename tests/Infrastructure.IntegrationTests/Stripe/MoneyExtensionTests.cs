namespace FitPass.Infrastructure.IntegrationTests.Stripe;

public class MoneyExtensionTests
{
    [TestCase("bif", 1000, true)]
    public void ShouldReturnStripeAmount(string currency, decimal amount, bool isZeroDecimal) { }
}
