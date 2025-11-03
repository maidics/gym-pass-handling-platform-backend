namespace FitPass.Domain.Exceptions;

public class InvalidCurrencyException : Exception
{
    public InvalidCurrencyException(string currency) : base($"Currency \"{currency}\" is unsupported.") { }
}