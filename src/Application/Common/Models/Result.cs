namespace FitPass.Application.Common.Models;

//Result object to pass around instead of throwing Exceptions
//used starting from Application Layer 
//if something fails then that Result will be converted to http error or handled when it comes to that specific case
//should I differentiate between certain types of Internal Server Errors?
//logger would already log it so client only needs a user friendly error message anyways
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

    public static ResultFailure NotFound(string parameterName, IEnumerable<string> errors = default!) =>
        new ResultFailure(ResultTypes.NotFound, $"'{parameterName}' not found.", [..errors ?? []]);

    public static ResultFailure Conflict(string parameterName, IEnumerable<string> errors = default!) =>
        new ResultFailure(ResultTypes.Conflict, $"'{parameterName}' is already taken.", [..errors ?? []]);

    public static ResultFailure ExternalServiceError(string externalServiceName, IEnumerable<string> errors = default!) =>
        new ResultFailure(ResultTypes.ExternalServiceUnavailable, $"'{externalServiceName}' is currently not available.", [..errors ?? []]);

    public static ResultFailure BusinessRuleViolation(IEnumerable<string> errors = default!) =>
        new ResultFailure(ResultTypes.BusinessRuleViolation, "Business rule violation.", [..errors ?? []]);

    public static ResultFailure InternalError(string? message = default, IEnumerable<string> errors = default!) => 
        new ResultFailure(ResultTypes.InternalError, message is null ? "Internal server error." : message, [..errors ?? []]);

    public static ResultFailure PaymentRequired(IEnumerable<string> errors = default!) =>
        new ResultFailure(ResultTypes.PaymentRequired, "Payment required.", [..errors ?? []]);

    public static ResultFailure Unauthorized(string? message = default) => 
        new ResultFailure(ResultTypes.Unauthorized, message is null ? "Unauthorized access." : message, []);

    public static ResultFailure Forbidden(string? message = default) => 
        new ResultFailure(ResultTypes.Forbidden, message is null ? "Forbidden access." : message, []);
}

public class Result<T> : Result
{
    private readonly T _value;
    protected Result(bool succeeded, string message, IEnumerable<string> errors, T value, ResultTypes type)
        : base(succeeded, message, errors, type)
    {
        _value = value;
    }

    protected Result(ResultFailure failure) : base(failure)
    {
        _value = default!;
    }

    public T Value
    {
        get
        {
            if (!Succeeded)
            {
                throw new InvalidOperationException("Failed result does not have inner value.");
            }

            return _value;
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
        return new Result<T2>(
            succeeded: false,
            message: Message,
            errors: Errors,
            value: default!,
            type: Type
        );
    }
}

public enum ResultTypes
{
    Success, //Ok, NoContent
    NotFound,
    Conflict,
    ExternalServiceUnavailable, //TypedResults.Problem
    BusinessRuleViolation, //BadRequest
    InternalError, //internal server error
    PaymentRequired,
    Unauthorized,
    Forbidden
}

public class ResultFailure
{
    public ResultTypes Type { get; }
    public string Message { get; }
    public string[] Errors { get; }

    public ResultFailure(ResultTypes type, string message, IEnumerable<string> errors)
    {
        Type = type;
        Message = message;
        Errors = errors.ToArray();
    }
}