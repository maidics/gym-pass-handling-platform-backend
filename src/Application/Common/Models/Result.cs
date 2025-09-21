namespace FitPass.Application.Common.Models;

public class Result
{
    internal Result(bool succeeded, IEnumerable<string> errors)
    {
        Succeeded = succeeded;
        ErrorMessages = errors.ToArray();
    }

    public bool Succeeded { get; init; }

    public string[] ErrorMessages { get; init; }

    public static Result Success()
    {
        return new Result(true, []);
    }

    public static Result Failure(IEnumerable<string> errors)
    {
        return new Result(false, errors);
    }
}


public class Result<TValue>
{
    internal Result(bool succeeded, TValue? value, IEnumerable<string> errors)
    {
        Succeeded = succeeded;
        Value = value;
        ErrorMessages = errors.ToArray();
    }

    public bool Succeeded { get; init; }

    public TValue? Value { get; init; }

    public string[] ErrorMessages { get; init; }

    public static Result<TValue> Success(TValue value)
    {
        return new Result<TValue>(true, value, []);
    }

    public static Result<TValue> Failure(IEnumerable<string> errors)
    {
        return new Result<TValue>(false, default, errors);
    }
}
