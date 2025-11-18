namespace FitPass.Application.Common.Models;

public class Result
{
    private Result(bool succeeded, IEnumerable<string> errors)
    {
        Succeeded = succeeded;
        Errors = errors.ToArray();
    }

    public bool Succeeded { get; }
    public string[] Errors { get; }

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
    private readonly TValue _value;
    private Result(bool succeeded, IEnumerable<string> errors, TValue value)
    {
        Succeeded = succeeded;
        Errors = errors.ToArray();
        _value = value;
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

    public static Result<TValue> Success(TValue value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return new Result<TValue>(true, [], value);
    }

    public static Result<TValue> Failure(IEnumerable<string> errors, Exception? exception = null)
    {
        return new Result<TValue>(false, errors, default!);
    }
}

public class Result<TValue, TEnum> where TEnum : Enum
{
    private readonly TValue _value;
    private readonly TEnum _type;
    private Result(bool succeeded, IEnumerable<string> errors, TValue value, TEnum type)
    {
        Succeeded = succeeded;
        Errors = errors.ToArray();
        _value = value;
        _type = type;
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

    public TEnum Type 
    { 
        get 
        { 
            if (Type is null)
            {
                throw new ArgumentNullException(nameof(Type));
            }

            return _type;
        } 
    }

    public static Result<TValue, TEnum> Success(TValue value, TEnum successType)
    {
        ArgumentNullException.ThrowIfNull(value);
        return new Result<TValue, TEnum>(true, [], value, successType);
    }

    public static Result<TValue, TEnum> Failure(IEnumerable<string> errors, TEnum failureType)
    {
        return new Result<TValue, TEnum>(false, errors, default!, failureType);
    }
}
