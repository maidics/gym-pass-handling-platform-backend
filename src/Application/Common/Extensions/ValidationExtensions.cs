using FitPass.Domain.Constants;

namespace FitPass.Application.Extensions;

public static class ValidationExtensions
{
    public static IRuleBuilderOptions<T, TProperty> NotEmptyWithMessage<T, TProperty>(this IRuleBuilder<T, TProperty> rule, string propertyName)
    {
        return rule
            .NotEmpty()
            .WithMessage($"{propertyName} is required.");
    }

    public static IRuleBuilderOptions<T, string> StrongPassword<T>(this IRuleBuilder<T, string> rule)
    {
        return rule
            .NotEmptyWithMessage("Password")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters long.")
            .MaximumLength(MaxStringLengths.Password).WithMessage($"Password cannot be longer than {MaxStringLengths.Password}  characters.")
            .Must(p => p.Any(char.IsLower)).WithMessage("Password must contain at least one lowercase letter.")
            .Must(p => p.Any(char.IsUpper)).WithMessage("Password must contain at least one uppercase letter.")
            .Must(p => p.Any(char.IsDigit)).WithMessage("Password must contain at least one number.")
            .Must(p => p.Any(c => !char.IsLetterOrDigit(c))).WithMessage("Password must contain at least one special character.");
    }

    public static IRuleBuilderOptions<T, string> MaxLengthWithMessage<T>(this IRuleBuilder<T, string> rule, int maxLength, string propertyName)
    {
        return rule
            .MaximumLength(maxLength)
            .WithMessage($"{propertyName} cannot be longer than {maxLength} characters.");
    }

    public static IRuleBuilderOptions<T, string> NotEmptyWithMaxLenghtAndMessage<T>(this IRuleBuilder<T, string> rule, int maxLength, string propertyName)
    {
        return rule
            .NotEmptyWithMessage(propertyName)
            .MaxLengthWithMessage(maxLength, propertyName);
    }

    public static IRuleBuilder<T, string> PhoneNumber<T>(this IRuleBuilder<T, string> rule, string propertyName)
    {
        return rule
            .NotEmptyWithMaxLenghtAndMessage(MaxStringLengths.PhoneNumber, propertyName)
            .Matches(@"^\+?[0-9]{7,15}$").WithMessage($"Phone number must contain 7 to 15 digits, with an optional '+' prefix and it must not contain any spaces or other special characters.");
    }

    public static IRuleBuilder<T, TEnum> IsInEnumWithMessage<T, TEnum>(this IRuleBuilder<T, TEnum> rule, string propertyName) where TEnum : Enum
    {
        return rule
            .IsInEnum().WithMessage($"Provided {propertyName} is not valid.");
    }
}
