using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Resources;
using FitPass.Domain.Constants;

namespace FitPass.Application.Common.Extensions;

public static class ValidationExtensions
{
    public static IRuleBuilderOptions<T, TProperty> NotEmptyWithMessageLocalized<T, TProperty>(this IRuleBuilder<T, TProperty> rule, ILocalizer localizer, string key)
    {
        return rule
            .NotEmpty()
            .WithMessage(localizer.GetWithParamsLocalized(nameof(SharedResource.PropertyIsRequired), key));
    }

    public static IRuleBuilderOptions<T, TProperty> PropertyOfEntityNotEmptyWithMessageLocalized<T, TProperty>(
        this IRuleBuilder<T, TProperty> rule, ILocalizer localizer, string propertyKey, string entityKey)
    {
        return rule
            .NotEmpty()
            .WithMessage(localizer.GetPropertyOfEntityIsRequired(propertyKey, entityKey));
    }

    public static IRuleBuilderOptions<T, string> NotEmptyWithMaxLengthAndMessageLocalized<T>(
        this IRuleBuilder<T, string> rule, ILocalizer localizer, string key, int maxLength)
    {
        return rule
            .NotEmpty()
            .WithMessage(localizer.GetWithParamsLocalized(nameof(SharedResource.PropertyIsRequired), key, maxLength.ToString()))
            .MaximumLength(maxLength)
            .WithMessage(localizer.GetWithParamsLocalized(nameof(SharedResource.PropertyMaxLength), key, maxLength.ToString()));
    }

    public static IRuleBuilderOptions<T, string> NotEmptyWithMinLengthAndMessageLocalized<T>(
        this IRuleBuilder<T, string> rule, ILocalizer localizer, string key, int minLength)
    {
        return rule
            .NotEmpty()
            .WithMessage(localizer.GetWithParamsLocalized(nameof(SharedResource.PropertyIsRequired), key, minLength.ToString()))
            .MinimumLength(minLength)
            .WithMessage(localizer.GetWithParamsLocalized(nameof(SharedResource.PropertyMinLength), key, minLength.ToString()));
    }
    
    public static IRuleBuilderOptions<T, string> StrongPasswordLocalized<T>(this IRuleBuilder<T, string> rule, ILocalizer localizer)
    {
        return rule
            .NotEmptyWithMessageLocalized(localizer, nameof(SharedResource.Password))
            .MinimumLength(8).WithMessage(localizer.Get(nameof(SharedResource.PasswordMinimumLength), 8))
            .MaximumLength(MaxLength.Password).WithMessage(localizer.Get(nameof(SharedResource.PasswordMaximumLength), MaxLength.Password))
            .Must(p => p.Any(char.IsLower)).WithMessage(localizer.Get(nameof(SharedResource.PasswordAtLeastOneLowerCase)))
            .Must(p => p.Any(char.IsUpper)).WithMessage(localizer.Get(nameof(SharedResource.PasswordAtLeastOneUpperCase)))
            .Must(p => p.Any(char.IsDigit)).WithMessage(localizer.Get(nameof(SharedResource.PasswordAtLeastOneNumber)))
            .Must(p => p.Any(c => !char.IsLetterOrDigit(c))).WithMessage(localizer.Get(nameof(SharedResource.PasswordAtLeastOneSpecial)));
    }

    public static IRuleBuilder<T, string> EmailAddressWithMessageLocalized<T>(this IRuleBuilder<T, string> rule, ILocalizer localizer)
    {
        return rule
            .NotEmptyWithMessageLocalized(localizer, nameof(SharedResource.Email))
            .EmailAddress()
            .WithMessage(localizer.GetWithParamsLocalized(nameof(SharedResource.ValueIsInvalid), nameof(SharedResource.Email)));
    }

    public static IRuleBuilder<T, string> SupportedLanguageWithMessageLocalized<T>(this IRuleBuilder<T, string> rule,
        ILocalizer localizer, string[] supportedLanguages)
    {
        return rule
            .Must(lang => supportedLanguages.Contains(lang))
            .WithMessage(localizer.Get(nameof(SharedResource.LanguageIsNotSupported), string.Join(", ", supportedLanguages)));
    }
}
