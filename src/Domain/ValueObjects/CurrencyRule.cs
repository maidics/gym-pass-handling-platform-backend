namespace FitPass.Domain.ValueObjects;

public sealed record CurrencyRule(
    CurrencyCode CurrencyCode, 
    int MinorUnits, // decimals: EUR, HUF, USD all 2 decimal currencies
    decimal MinAmount);
