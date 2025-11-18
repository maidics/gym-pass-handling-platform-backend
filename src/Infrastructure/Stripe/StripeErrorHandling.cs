using System.Net;
using FitPass.Application.Common.Models;
using FitPass.Domain.Enums;
using Microsoft.Extensions.Logging;
using Stripe;

namespace FitPass.Infrastructure.Stripe;

public static class StripeExceptionExtensions
{
    public static Result<TValue, PaymentProviderResult> ToApplicationResult<TValue>(this StripeException stripeException)
    {
        return stripeException.HttpStatusCode switch
        {
            HttpStatusCode.TooManyRequests => Result<TValue, PaymentProviderResult>.Failure([stripeException.StripeError.Message], PaymentProviderResult.TooManyRequests),

            HttpStatusCode.PaymentRequired => Result<TValue, PaymentProviderResult>.Failure([stripeException.StripeError.Message], PaymentProviderResult.PaymentRequired),

            HttpStatusCode.BadRequest or
            HttpStatusCode.Unauthorized or
            HttpStatusCode.Forbidden or
            HttpStatusCode.NotFound => Result<TValue, PaymentProviderResult>.Failure([stripeException.StripeError.Message], PaymentProviderResult.InvalidRequest),

            _ => Result<TValue, PaymentProviderResult>.Failure([stripeException.StripeError.Message], PaymentProviderResult.Unexpected)
        };
    }

    public static void Log(this StripeException stripeException, ILogger logger, string serviceClassName, string serviceClassMethodName)
    {
        _logStripeException(logger, serviceClassName, serviceClassMethodName, stripeException);
    }

    private static readonly Action<ILogger, string, string, Exception> _logStripeException =
        LoggerMessage.Define<string, string>(
            logLevel: LogLevel.Error,
            eventId: new EventId(),
            formatString: "Caught StripeException in {ServiceClass}.{ServiceClassMethod}");
} 

