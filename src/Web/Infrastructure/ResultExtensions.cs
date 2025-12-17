using FitPass.Application.Common.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace FitPass.Web.Infrastructure;

public static class ResultExtensions
{
    public static Results<Ok<T>, ProblemHttpResult> ToTypedResult<T>(this Result<T> result)
    {
        if (result.Succeeded)
        {
            return TypedResults.Ok(result.Value);
        }

        return result.MapResultFailure();
    }

    public static Results<NoContent, ProblemHttpResult> ToTypedResult(this Result result)
    {
        if (result.Succeeded)
        {
            return TypedResults.NoContent();
        }

        return result.MapResultFailure();
    }

    private static ProblemHttpResult MapResultFailure(this Result result)
    {
        //???? : TypedResults.NotFound(CreateProblemDetails(result));

        return TypedResults.Problem(CreateProblemDetails(result));
    } 

    private static int GetStatusCode(this ResultTypes resultTypes)
    {
        return resultTypes switch
        {
            ResultTypes.NotFound => StatusCodes.Status404NotFound,
            ResultTypes.Conflict => StatusCodes.Status409Conflict,
            ResultTypes.ExternalServiceUnavailable => StatusCodes.Status503ServiceUnavailable,
            ResultTypes.BusinessRuleViolation => StatusCodes.Status400BadRequest,
            ResultTypes.InternalError => StatusCodes.Status500InternalServerError,
            ResultTypes.PaymentRequired => StatusCodes.Status402PaymentRequired,
            ResultTypes.Unauthorized => StatusCodes.Status401Unauthorized,
            ResultTypes.Forbidden => StatusCodes.Status403Forbidden,
            _ => StatusCodes.Status500InternalServerError
        };
    }

    private static ProblemDetails CreateProblemDetails(Result result)
    {
        var problemDetails = new ProblemDetails
        {
            Status = result.Type.GetStatusCode(),
            Title = result.Type.ToString(),
            Detail = result.Message
        };

        if (result.Errors is not null && result.Errors.Length != 0)
        {
            problemDetails.Extensions["errors"] = result.Errors;
        }

        return problemDetails;
    }
}
