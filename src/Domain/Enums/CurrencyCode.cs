using FitPass.Domain.Constants;

namespace FitPass.Domain.Enums;

public enum CurrencyCode
{
    USD,
    EUR,
    HUF
}

public static class CurrencyCodeExtensions
{
    extension(CurrencyCode currency)
    {
        public bool IsZeroDecimal()
        {
            return CurrencyPolicies.Rules.Count(x => x.Value.MinorUnits == 0 && x.Key == currency) > 0;
        }

        public int GetMinorUnits()
        {
            return CurrencyPolicies.GetRule(currency).MinorUnits;
        }
        
        public decimal GetMinAmount()
        {
            return CurrencyPolicies.GetRule(currency).MinAmount;
        }
    }
}
