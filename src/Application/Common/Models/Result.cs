using FitPass.Domain.Strings;

namespace FitPass.Application.Common.Models;

public class Result
{
    internal Result(bool succeeded, IEnumerable<string> errors)
    {
        Succeeded = succeeded;
        Errors = errors.ToArray();
    }

    public bool Succeeded { get; init; }
    public string[] Errors { get; init; }

    public static Result Success()
    {
        return new Result(true, []);
    }

    public static Result Failure(IEnumerable<string> errors)
    {
        return new Result(false, errors);
    }

    public bool IsUserNotFoundFailure()
    {
        return !Succeeded && Errors.Length == 1 && Errors.First() == ErrorMessages.UserNotFound();
    }
}
