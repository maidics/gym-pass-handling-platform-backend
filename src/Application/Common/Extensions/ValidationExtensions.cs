using FitPass.Domain.Constants;
using FitPass.Domain.Strings;

namespace FitPass.Application.Common.Extensions;

public static class ValidationExtensions
{
    public static IRuleBuilderOptions<T, TProperty> NotEmptyWithMessage<T, TProperty>(this IRuleBuilder<T, TProperty> rule, string propertyName)
    {
        return rule
            .NotEmpty()
            .WithMessage(ErrorMessages.PropertyIsRequired(propertyName));
    }

    public static IRuleBuilderOptions<T, string> StrongPassword<T>(this IRuleBuilder<T, string> rule)
    {
        return rule
            .NotEmptyWithMessage("Password")
            .MinimumLength(8).WithMessage(ErrorMessages.PasswordMinimumLength())
            .MaximumLength(MaxStringLengths.Password).WithMessage(ErrorMessages.PasswordMaximumLength())
            .Must(p => p.Any(char.IsLower)).WithMessage(ErrorMessages.PasswordAtLeastOneLowerCase())
            .Must(p => p.Any(char.IsUpper)).WithMessage(ErrorMessages.PasswordAtLeastOneUpperCase())
            .Must(p => p.Any(char.IsDigit)).WithMessage(ErrorMessages.PasswordAtLeastOneNumber())
            .Must(p => p.Any(c => !char.IsLetterOrDigit(c))).WithMessage(ErrorMessages.PasswordAtLeastOneSpecial());
    }

    public static IRuleBuilderOptions<T, string> MaxLengthWithMessage<T>(this IRuleBuilder<T, string> rule, string propertyName, int maxLength)
    {
        return rule
            .MaximumLength(maxLength).WithMessage(ErrorMessages.PropertyCannotBeLongerThan(propertyName, maxLength));
    }

    public static IRuleBuilderOptions<T, string> MinimumLengthWithMessage<T>(this IRuleBuilder<T, string> rule, string propertyName, int minLength)
    {
        return rule
            .MinimumLength(minLength).WithMessage(ErrorMessages.PropertyMustBeAtLeastLength(propertyName, minLength));
    }

    public static IRuleBuilderOptions<T, string> NotEmptyWithMaxLenghtAndMessage<T>(this IRuleBuilder<T, string> rule, string propertyName, int maxLength)
    {
        return rule
            .NotEmptyWithMessage(propertyName)
            .MaxLengthWithMessage(propertyName, maxLength);
    }

    public static IRuleBuilderOptions<T, string> NotEmptyWithMinimumLength<T>(this IRuleBuilder<T, string> rule, string propertyName, int minLength)
    {
        return rule
            .NotEmptyWithMessage(propertyName)
            .MinimumLengthWithMessage(propertyName, minLength);
    }

    public static IRuleBuilder<T, string> PhoneNumber<T>(this IRuleBuilder<T, string> rule, string propertyName)
    {
        return rule
            .NotEmptyWithMaxLenghtAndMessage(propertyName, MaxStringLengths.PhoneNumber)
            .Matches(@"^\+?[0-9]{7,15}$").WithMessage(ErrorMessages.InvalidPhoneNumber());
    }

    public static IRuleBuilder<T, string> ValidEmailAddress<T>(this IRuleBuilder<T, string> rule, string emailPropertyName)
    {
        return rule
            .NotEmptyWithMaxLenghtAndMessage(emailPropertyName, MaxStringLengths.Email)
            .EmailAddress().WithMessage(ErrorMessages.InvalidEmailAddress(emailPropertyName));
    }

    public static IRuleBuilder<T, string>IsApplicationRole<T>(this IRuleBuilder<T, string> rule, string propertyName)
    {
        return rule
            .NotEmptyWithMessage(ErrorMessages.PropertyIsRequired(propertyName))
            .Must(Roles.IsValidRole).WithMessage((_, invalidRole) => ErrorMessages.InvalidRole(invalidRole));
    }
}
