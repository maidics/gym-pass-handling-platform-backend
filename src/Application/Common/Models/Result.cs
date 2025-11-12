namespace FitPass.Application.Common.Models;

public class Result
{
    private readonly Exception? _exception;

    private Result(bool succeeded, IEnumerable<string> errors, Exception? exception)
    {
        Succeeded = succeeded;
        Errors = errors.ToArray();
        _exception = exception;
    }

    public bool Succeeded { get; }
    public string[] Errors { get; }
    public Exception? Exception
    {
        get
        {
            if (Succeeded)
            {
                throw new InvalidOperationException("Succeded result does not have an exception.");
            }

            return _exception;
        }
    }

    public static Result Success()
    {
        return new Result(true, [], null);
    }

    public static Result Failure(IEnumerable<string> errors, Exception? exception = null)
    {
        return new Result(false, errors, exception);
    }
}

public class Result<TValue>
{
    private readonly TValue _value;
    private readonly Exception? _exception;
    private Result(bool succeeded, IEnumerable<string> errors, TValue value, Exception? exception)
    {
        Succeeded = succeeded;
        Errors = errors.ToArray();
        _value = value;
        _exception = exception;
    }

    public bool Succeeded { get; }
    public string[] Errors { get; }
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

    public Exception? Exception
    {
        get
        {
            if (Succeeded)
            {
                throw new InvalidOperationException("Succeded result does not have an exception.");
            }

            return _exception;
        }
    }

    public static Result<TValue> Success(TValue value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return new Result<TValue>(true, [], value, null);
    }

    public static Result<TValue> Failure(IEnumerable<string> errors, Exception? exception = null)
    {
        return new Result<TValue>(false, errors, default!, exception);
    }
}

public class Result<TValue, TFailureEnum> where TFailureEnum : Enum
{
    private readonly TValue _value;
    private readonly Exception? _exception;
    private readonly TFailureEnum _failureType;
    private Result(bool succeeded, IEnumerable<string> errors, TValue value, TFailureEnum failureType, Exception? exception)
    {
        Succeeded = succeeded;
        Errors = errors.ToArray();
        _value = value;
        _exception = exception;
        _failureType = failureType;
    }

    public bool Succeeded { get; }
    public string[] Errors { get; }
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

    public Exception? Exception
    {
        get
        {
            if (Succeeded)
            {
                throw new InvalidOperationException("Succeded result does not have an exception.");
            }

            return _exception;
        }
    }

    public TFailureEnum FailureType 
    { 
        get 
        { 
            if (Succeeded)
            {
                throw new InvalidOperationException("Succeeded result does not have a failure type.");
            }

            return _failureType;
        } 
    }

    public static Result<TValue, TFailureEnum> Success(TValue value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return new Result<TValue, TFailureEnum>(true, [], value, default!, null);
    }

    public static Result<TValue, TFailureEnum> Failure(IEnumerable<string> errors, TFailureEnum failureType, Exception? exception = null)
    {
        return new Result<TValue, TFailureEnum>(false, errors, default!, failureType, exception);
    }
}
