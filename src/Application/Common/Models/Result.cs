namespace FitPass.Application.Common.Models;

//Result object to pass around instead of throwing Exceptions
//used starting from Application Layer 
//if something fails then that Result will be converted to http error or handled when it comes to that specific case
//should I differentiate between certain types of Internal Server Errors?
//logger would already log it so client only needs a user friendly error message anyways
public class Result
{
    private Result(bool succeeded, IEnumerable<string> errors, ResultType type)
    {
        Succeeded = succeeded;
        Errors = errors.ToArray();
        Type = type;
    }

    public bool Succeeded { get; }
    public string[] Errors { get; }
    public ResultType Type { get; }

    public static Result Success()
    {
        return new Result(true, [], ResultType.Success);
    }

    public static Result Failure(IEnumerable<string> errors, ResultType type)
    {
        return new Result(false, errors, type);
    }
}

public class Result<TValue>
{
    private readonly TValue _value;
    private Result(bool succeeded, IEnumerable<string> errors, TValue value, ResultType type)
    {
        Succeeded = succeeded;
        Errors = errors.ToArray();
        _value = value;
        Type = type;
    }

    public bool Succeeded { get; }
    public string[] Errors { get; }
    public ResultType Type { get; }
    public TValue Value
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

    public static Result<TValue> Success(TValue value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return new Result<TValue>(true, [], value, ResultType.Success);
    }

    public static Result<TValue> Failure(IEnumerable<string> errors, ResultType type)
    { 
        return new Result<TValue>(false, errors, default!, type);
    }
}

public enum ResultType
{
    Success, //Ok, NoContent
    NotFound,
    Conflict,
    ExternalServiceInvalidCall, //InternalServerError
    ExternalServiceUnavailable, //ExternalServiceUnavailable => TypedResults.Problem
    BusinessRuleViolation,
    InternalError, //internal server error
    PaymentRequired,
    Unauthorized,
    Forbidden
}
