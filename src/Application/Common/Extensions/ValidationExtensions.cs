namespace FitPass.Application.Extensions;

public static class ValidationExtensions
{
    public static IRuleBuilderOptions<T, TProperty> NotEmptyWithMessage<T, TProperty>(this IRuleBuilder<T, TProperty> rule, string message)
    {
        return rule.NotEmpty().WithMessage(message);
    }

    public static IRuleBuilderOptions<T, string> StrongPassword<T, string>(this IRuleBuilder<T, string> rule) {
        return rule
            .MinimumLength(10).WithMessage("'{PropertyName}' must be at least 10 characters long.")
            .Must(p => p.Any(char.IsLower)).WithMessage("'{PropertyName}' must contain at least one lowercase letter.")
            .Must(p => p.Any(char.IsUpper)).WithMessage("'{PropertyName}' must contain at least one uppercase letter.")
            .Must(p => p.Any(char.IsDigit)).WithMessage("'{PropertyName}' must contain at least one number.")
            .Must(p => p.Any(c => !char.IsLetterOrDigit(c))).WithMessage("'{PropertyName}' must contain at least one special character.");
    }
}