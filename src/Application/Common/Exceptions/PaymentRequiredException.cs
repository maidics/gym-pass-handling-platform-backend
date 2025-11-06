namespace FitPass.Application.Common.Exceptions;

public class PaymentRequiredException(string message) : Exception(message);