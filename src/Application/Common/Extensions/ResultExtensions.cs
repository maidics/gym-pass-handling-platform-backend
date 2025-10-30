using FitPass.Application.Common.Models;

namespace FitPass.Application.Common.Extensions;

public static class ResultExtensions
{
    public static bool IsResultFailureWithOneErrorMessage(this Result result, string errorMessage)
    {
        return !result.Succeeded && result.Errors.Length == 1 && result.Errors.First() == errorMessage;
    }
}
