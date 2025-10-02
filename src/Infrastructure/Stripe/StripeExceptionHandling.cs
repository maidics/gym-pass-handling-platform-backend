using System.Net;
using Stripe;

namespace FitPass.Infrastructure.Stripe;

public enum StripeFailureType
{
    Retryable, //5xx, 429, Network Errors
    PaymentDeclined, //402 Card Error (User's fault)
    InvalidRequest, //400, 401, 403, 404 (Dev's fault)
    UnexpectedError //everything else
}

public record StripeFailureDetails(StripeFailureType StripeFailureType, string UserErrorMessage, Exception OriginalException);

public static class StripeExceptionClassifier
{
    public static StripeFailureDetails Classify(this StripeException ex)
    {
        string devMadeErrorMessage = $"Stripe configuration or request error occured. Please contact support.";
        string userMadeErrorMessage = "The payment was declined by the bank. Please check card info or try another card.";
        string temporalErrorMessage = "A temporary issue occured while processing your request. Please try again shortly.";
        string unexpectedErrorMessage = "An unexpected error occured, Please contact support.";

        return ex.HttpStatusCode switch
        {
            HttpStatusCode.TooManyRequests => new StripeFailureDetails(StripeFailureType.Retryable, temporalErrorMessage, ex),

            HttpStatusCode.PaymentRequired => new StripeFailureDetails(StripeFailureType.PaymentDeclined, userMadeErrorMessage, ex),

            HttpStatusCode.BadRequest or
            HttpStatusCode.Unauthorized or
            HttpStatusCode.Forbidden or
            HttpStatusCode.NotFound => new StripeFailureDetails(StripeFailureType.InvalidRequest, devMadeErrorMessage, ex),

            _ => new StripeFailureDetails(StripeFailureType.UnexpectedError, unexpectedErrorMessage, ex),
        };
    }

    public static string ToErrorString(this StripeException ex)
    {
        return $"Caught StripeException: {ex.Message}" +
            $"\nStripeError: " +
                $"\n\tType: {ex.StripeError.Type} " +
                $"\n\tError: {ex.StripeError.Error}" +
                $"\n\tErrorDescription: {ex.StripeError.ErrorDescription}" +
                $"\n\tParam: {ex.StripeError.Param}" + 
                $"\n\tCode: {ex.StripeError.Code}" + 
                $"\n\tCharge: {ex.StripeError.Charge}" + 
                $"\n\tPaymentIntent: {ex.StripeError.PaymentIntent}" + 
                $"\n\tPaymentMethod: {ex.StripeError.PaymentMethod}" + 
                $"\n\tPaymentMethodType: {ex.StripeError.PaymentMethodType}" + 
                $"\n\tStripeError Source:" +
                    $"\n\t\tId: {ex.StripeError.Source.Id}" + 
                    $"\n\t\tObject: {ex.StripeError.Source.Object}" + 
                    $"\n\t\tStripeResponse: {ex.StripeError.Source.StripeResponse}";
    }
}