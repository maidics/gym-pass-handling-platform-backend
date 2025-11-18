using System.Net;
using FitPass.Application.Common.Models;
using FitPass.Domain.Enums.Payment;
using Microsoft.Extensions.Logging;
using Stripe;

namespace FitPass.Infrastructure.Stripe;

public static class StripeExceptionExtensions
{
    public static Result<TValue> ToApplicationResult<TValue>(this StripeException stripeException)
    {
        return stripeException.HttpStatusCode switch
        {
            HttpStatusCode.TooManyRequests => Result<TValue>.Failure([stripeException.StripeError.Message], ResultType.ExternalServiceUnavailable),

            HttpStatusCode.PaymentRequired => Result<TValue>.Failure([stripeException.StripeError.Message], ResultType.PaymentRequired),

            HttpStatusCode.BadRequest or
            HttpStatusCode.Unauthorized or
            HttpStatusCode.Forbidden or
            HttpStatusCode.NotFound => Result<TValue>.Failure([stripeException.StripeError.Message], ResultType.InternalError),

            _ => Result<TValue>.Failure([stripeException.StripeError.Message], ResultType.InternalError)
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

