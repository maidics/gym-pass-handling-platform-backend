using FitPass.Application.Common.Constants;
using FitPass.Application.Common.Interfaces;
using FitPass.Domain.Constants;

namespace FitPass.Application.Common.Extensions;

public static class ValidationExtensions
{
    public static IRuleBuilderOptions<T, TProperty> NotEmptyLocalized<T, TProperty>(this IRuleBuilder<T, TProperty> rule, ILocalizer localizer, string propertyNameKey)
    {
        return rule
            .NotEmpty()
            .WithMessage(localizer.Get(LocalizationKeys.PropertyIsRequired, localizer.Get(propertyNameKey)));
    }

    public static IRuleBuilderOptions<T, string> StrongPasswordLocalized<T>(this IRuleBuilder<T, string> rule, ILocalizer localizer)
    {
        return rule
            .NotEmptyLocalized(localizer, LocalizationKeys.Password)
            .MinimumLength(8).WithMessage(localizer.Get(LocalizationKeys.PasswordMinimumLength, 8))
            .MaximumLength(MaxStringLengths.Password).WithMessage(localizer.Get(LocalizationKeys.PasswordMaximumLength, MaxStringLengths.Password))
            .Must(p => p.Any(char.IsLower)).WithMessage(localizer.Get(LocalizationKeys.PasswordAtLeastOneLowerCase))
            .Must(p => p.Any(char.IsUpper)).WithMessage(localizer.Get(LocalizationKeys.PasswordAtLeastOneUpperCase))
            .Must(p => p.Any(char.IsDigit)).WithMessage(localizer.Get(LocalizationKeys.PasswordAtLeastOneNumber))
            .Must(p => p.Any(c => !char.IsLetterOrDigit(c))).WithMessage(localizer.Get(LocalizationKeys.PasswordAtLeastOneSpecial));
    }

    public static IRuleBuilderOptions<T, string> MaxLengthWithMessageLocalized<T>(this IRuleBuilder<T, string> rule, ILocalizer localizer, string propertyNameKey, int maxLength)
    {
        return rule
            .MaximumLength(maxLength).WithMessage(
                localizer.Get(
                    LocalizationKeys.PropertyCannotBeLongerThan,
                    localizer.Get(propertyNameKey), 
                    maxLength));
    }

    public static IRuleBuilderOptions<T, string> MinimumLengthWithMessageLocalized<T>(this IRuleBuilder<T, string> rule, ILocalizer localizer, string propertyNameKey, int minLength)
    {
        return rule
            .MinimumLength(minLength).WithMessage(
                localizer.Get(
                    LocalizationKeys.PropertyMustBeAtLeastLength,
                    localizer.Get(propertyNameKey),
                    minLength));
    }

    public static IRuleBuilderOptions<T, string> NotEmptyWithMaxLenghtAndMessageLocalized<T>(this IRuleBuilder<T, string> rule, ILocalizer localizer, string propertyNameKey, int maxLength)
    {
        return rule
            .NotEmptyLocalized(localizer, propertyNameKey)
            .MaxLengthWithMessageLocalized(localizer, propertyNameKey, maxLength);
    }

    public static IRuleBuilderOptions<T, string> NotEmptyWithMinimumLengthLocalized<T>(this IRuleBuilder<T, string> rule, ILocalizer localizer, string propertyNameKey, int minLength)
    {
        return rule
            .NotEmptyLocalized(localizer, propertyNameKey)
            .MinimumLengthWithMessageLocalized(localizer, propertyNameKey, minLength);
    }

    public static IRuleBuilder<T, string> PhoneNumberWithMessageLocalized<T>(this IRuleBuilder<T, string> rule, ILocalizer localizer)
    {
        return rule
            .NotEmptyWithMaxLenghtAndMessageLocalized(localizer, LocalizationKeys.PhoneNumber, MaxStringLengths.PhoneNumber)
            .Must(Domain.ValueObjects.PhoneNumber.IsValid).WithMessage(localizer.Get(LocalizationKeys.InvalidPhoneNumber));
    }

    public static IRuleBuilder<T, string> ValidEmailAddressWithMessageLocalized<T>(this IRuleBuilder<T, string> rule, ILocalizer localizer)
    {
        return rule
            .NotEmptyWithMaxLenghtAndMessageLocalized(localizer, LocalizationKeys.Email, MaxStringLengths.Email)
            .EmailAddress().WithMessage(localizer.Get(LocalizationKeys.InvalidEmailAddress));
    }
}
