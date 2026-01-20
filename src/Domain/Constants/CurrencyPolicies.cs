using System.Collections.Frozen;
using FitPass.Domain.ValueObjects;

namespace FitPass.Domain.Constants;

public static class CurrencyPolicies
{
    public static readonly FrozenDictionary<CurrencyCode, CurrencyRule> Rules =
        new Dictionary<CurrencyCode, CurrencyRule>()
        {
            { CurrencyCode.USD, new CurrencyRule(CurrencyCode.USD, 2, 1) },
            { CurrencyCode.EUR, new CurrencyRule(CurrencyCode.EUR, 2, 1) },
            { CurrencyCode.HUF, new CurrencyRule(CurrencyCode.HUF, 2, 250) }
        }.ToFrozenDictionary();

    public static CurrencyRule GetRule(CurrencyCode currency)
    {
        return Rules.TryGetValue(currency, out var rule)
            ? rule
            : throw new ArgumentException($"No {nameof(CurrencyRule)} found for '{currency}'.");
    }
}
