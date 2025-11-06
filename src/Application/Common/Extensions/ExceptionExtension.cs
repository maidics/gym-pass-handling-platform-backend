using System.Text.Json;
using FitPass.Application.Common.Exceptions;
using FitPass.Domain.Strings;

namespace FitPass.Application.Common.Extensions;

public static class ExceptionExtension
{
    public static bool IsStripeServiceException(this Exception exception)
    {
        return exception is PaymentRequiredException || exception is ExternalServiceUnavailableException || exception.Message == ErrorMessages.UnhandledErrorOccuredExternalService();
    }
    
    public static bool IsJsonSerializationException(this Exception exception)
    {
        return exception is JsonException || exception is NotSupportedException;
    }
}