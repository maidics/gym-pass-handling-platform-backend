using System.Net;
using FitPass.Application.Common.Models;
using FitPass.Domain.Enums;
using Microsoft.Extensions.Logging;
using Stripe;

namespace FitPass.Infrastructure.Stripe;

public static class StripeExceptionExtensions
{
    public static Result<TValue, PaymentFailure> ToApplicationResult<TValue>(this StripeException stripeException)
    {
        return stripeException.HttpStatusCode switch
        {
            HttpStatusCode.TooManyRequests => Result<TValue, PaymentFailure>.Failure([stripeException.StripeError.Message], PaymentFailure.TooManyRequests),

            HttpStatusCode.PaymentRequired => Result<TValue, PaymentFailure>.Failure([stripeException.StripeError.Message], PaymentFailure.PaymentRequired),

            HttpStatusCode.BadRequest or
            HttpStatusCode.Unauthorized or
            HttpStatusCode.Forbidden or
            HttpStatusCode.NotFound => Result<TValue, PaymentFailure>.Failure([stripeException.StripeError.Message], PaymentFailure.InternalServerError),

            _ => Result<TValue, PaymentFailure>.Failure([stripeException.StripeError.Message], PaymentFailure.Unexpected)
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

