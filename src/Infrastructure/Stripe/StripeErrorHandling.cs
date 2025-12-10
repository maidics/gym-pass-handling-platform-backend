using System.Net;
using FitPass.Application.Common.Models;
using Microsoft.Extensions.Logging;
using Stripe;

namespace FitPass.Infrastructure.Stripe;

public static class StripeExceptionExtensions
{
    public static ResultFailure ToResultFailure(this StripeException stripeException, string errorMessage)
    {
        return stripeException.HttpStatusCode switch
        {
            HttpStatusCode.TooManyRequests => new ResultFailure(ResultTypes.ExternalServiceUnavailable, errorMessage, []),

            HttpStatusCode.PaymentRequired => new ResultFailure(ResultTypes.PaymentRequired, errorMessage, []),

            /*
                HttpStatusCode.BadRequest or
                HttpStatusCode.Unauthorized or
                HttpStatusCode.Forbidden or
                HttpStatusCode.NotFound => 
            */

            _ => new ResultFailure(ResultTypes.InternalError, errorMessage, [])
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

public static class StripeHttpResponseHelper
{
    public static async ValueTask<bool> IsLockTimeoutAsync(HttpResponseMessage response)
    {
        if (response.StatusCode != HttpStatusCode.Conflict) return false;

        try
        {
            await response.Content.LoadIntoBufferAsync(); //from one time read to multiple reads

            var content = await response.Content.ReadAsStringAsync();
            
            return content.Contains("lock_timeout");
        }
        catch
        {
            return false;
        }
    }
}
