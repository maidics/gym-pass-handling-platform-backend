namespace FitPass.Application.Common.Models;

//Result object to pass around instead of throwing Exceptions
public class Result
{
    protected Result(bool succeeded, string message, IEnumerable<string> errors, ResultTypes type)
    {
        Succeeded = succeeded;
        Errors = errors.ToArray();
        Type = type;
        Message = message;
    }

    protected Result(ResultFailure failure)
    {
        Succeeded = false;
        Message = failure.Message;
        Errors = failure.Errors;
        Type = failure.Type;
    }

    public bool Succeeded { get; }
    public string Message { get; }
    public string[] Errors { get; }
    public ResultTypes Type { get; }

    public static Result Success()
    {
        return new Result(true, string.Empty, [], ResultTypes.Success);
    }

    public static Result<T> Success<T>(T value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return Result<T>.Success(value);
    }

    public static implicit operator Result(ResultFailure failure)
    {
        return new Result(failure);
    }

    public static Result Failure(string message, string[] errors, ResultTypes type)
    {
        return new Result(false, message, errors, type);
    } 

    public static ResultFailure NotFound(string? message, params string[] errors) =>
        new ResultFailure(ResultTypes.NotFound, message ?? "Resource not found.", errors);

    public static ResultFailure Conflict(string? message, params string[] errors) =>
        new ResultFailure(ResultTypes.Conflict, message ?? "Resource is already in use.", errors);

    public static ResultFailure ExternalServiceUnavailable(string? message, params string[] errors) =>
        new ResultFailure(ResultTypes.ExternalServiceUnavailable, message ?? "An external service is unavailable.", errors);

    public static ResultFailure BusinessRuleViolation(string? message, params string[] errors) =>
        new ResultFailure(ResultTypes.BusinessRuleViolation, message ?? "Business rule violation.", errors);

    public static ResultFailure InternalError(string? message, params string[] errors ) => 
        new ResultFailure(ResultTypes.InternalError, message ?? "Internal server error.", errors);

    public static ResultFailure PaymentRequired(string?  message, params string[] errors) =>
        new ResultFailure(ResultTypes.PaymentRequired, message ?? "Payment required.", errors);

    public static ResultFailure Unauthorized(string? message) => 
        new ResultFailure(ResultTypes.Unauthorized, message ?? "Unauthorized.", []);

    public static ResultFailure Forbidden(string? message) => 
        new ResultFailure(ResultTypes.Forbidden, message ?? "Forbidden access.", []);
}

public class Result<T> : Result
{
    private Result(bool succeeded, string message, IEnumerable<string> errors, T value, ResultTypes type)
        : base(succeeded, message, errors, type)
    {
        Value = value;
    }

    private Result(ResultFailure failure) : base(failure)
    {
        Value = default!;
    }

    public T Value
    {
        get
        {
            if (!Succeeded)
            {
                throw new InvalidOperationException("Failed result does not have inner value.");
            }

            return field;
        }
    }

    public static implicit operator Result<T>(ResultFailure failure)
    {
        return new Result<T>(failure);
    }

    public static Result<T> Success(T value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return new Result<T>(true, string.Empty, [], value, ResultTypes.Success);
    }

    public Result<T2> ToFailure<T2>()
    {
        if (Succeeded)
        {
            throw new InvalidOperationException("Cannot convert to new Result failure when Result is succeeded.");
        }

        return new Result<T2>(
            succeeded: false,
            message: Message,
            errors: Errors,
            value: default!,
            type: Type
        );
    }
}

public class ResultFailure
{
    public ResultTypes Type { get; }
    public string Message { get; }
    public string[] Errors { get; }

    public ResultFailure(ResultTypes type, string message, IEnumerable<string> errors)
    {
        ThrowIfNotFailedResult(type);

        Type = type;
        Message = message;
        Errors = errors.ToArray();
    }

    public ResultFailure(Result result)
    {
        ThrowIfNotFailedResult(result.Type);

        Type = result.Type;
        Message = result.Message;
        Errors = result.Errors.ToArray();
    }

    private static void ThrowIfNotFailedResult(ResultTypes type)
    {
        if (type == ResultTypes.Success)
        {
            throw new InvalidOperationException($"Cannot create {nameof(ResultFailure)} from {nameof(Result.Succeeded)} {nameof(Result)}");
        }
    }
}

public enum ResultTypes
{
    Success, //Ok, NoContent
    //Redirect,
    
    NotFound,
    Conflict,
    ExternalServiceUnavailable, //TypedResults.Problem
    BusinessRuleViolation, //BadRequest
    InternalError, //internal server error
    PaymentRequired,
    Unauthorized,
    Forbidden
}

/*
public static class ResultTypesExtensions
{
    private readonly static HashSet<ResultTypes> _successTypes =
    [
        ResultTypes.Success, ResultTypes.Redirect
    ];
    
    extension(ResultTypes type)
    {
        public bool IsSuccess()
        {
            return _successTypes.Contains(type);
        }
    }
}
*/
