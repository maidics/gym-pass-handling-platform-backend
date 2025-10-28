using FitPass.Domain.Constants;

namespace FitPass.Domain.Strings;

public static class ErrorMessages
{
    public static string PropertyIsRequired(string propertyName)
    {
        return $"'{propertyName}' is required.";
    }

    public static string PropertyCannotBeLongerThan(string propertyName, int maxLength)
    {
        return $"'{propertyName}' cannot be longer than {maxLength} characters.";
    }

    public static string PropertyMustBeAtLeastLength(string propertyName, int minimumLength)
    {
        return $"'{propertyName}' must be at least {minimumLength} characters.";
    }

    public static string InvalidEmailAddress(string emailPropertyName)
    {
        return $"Provided '{emailPropertyName}' is invalid.";
    }

    public static string InvalidPhoneNumber()
    {
        return "Phone number, it must contain 7 to 15 digits, with an optional '+' prefix and it must not contain any spaces or other special characters.";
    }

    public static string PropertyMustEqualToAnotherProperty(string property, string anotherProperty)
    {
        return $"'{property}' and '{anotherProperty}' must be the same";
    }

    public static string PropertyMustNotEqualToAnotherProperty(string property, string anotherProperty)
    {
        return $"'{property}' and '{anotherProperty}' must not be the same";
    }

    public static string PasswordMinimumLength()
    {
        return $"Password must be at least {MinStringLengths.Password} characters.";
    }

    public static string PasswordMaximumLength()
    {
        return $"Password cannot be longer than {MaxStringLengths.Password} characters.";
    }

    public static string PasswordAtLeastOneLowerCase()
    {
        return "Password must contain at least one lowercase character.";
    }

    public static string PasswordAtLeastOneUpperCase()
    {
        return "Password must contain at least one uppercase character.";
    }

    public static string PasswordAtLeastOneNumber()
    {
        return "Password must contain at least one number.";
    }

    public static string PasswordAtLeastOneSpecial()
    {
        return "Password must contain at least one special character";
    }

    public static string NotContainedByEnum(string enumName)
    {
        return $"Provided {enumName} is not valid.";
    }

    public static string SingleUsePassTypeOnlyOneUse()
    {
        return "Single use pass type most only have one total use.";
    }

    public static string SingleUsePassCannotExpire()
    {
        return "Single use pass type cannot expire.";
    }

    public static string MultiUsePassTypeAtLeastTwoUses()
    {
        return "Multi use pass type must have at least two uses.";
    }

    public static string MultiUsePassCannotExpire()
    {
        return "Multi use pass type cannot expire.";
    }

    public static string UnlimitedPassTypeExpirationDayAtleastOne()
    {
        return "Unlimited use pass type must expire at least after 1 day from today.";
    }

    public static string UnlimitedPassTypeNoUses()
    {
        return "Unlimited use pass type cannot have total uses.";
    }

    public static string PriceMustBePositive(string pricePropertyName)
    {
        return $"'{pricePropertyName}' has to be a positive number";
    }

    public static string PropertyCannotBeNullIfAnotherIsNull(string propertyName, string anotherPropertyName)
    {
        return $"'{propertyName}' cannot be null if '{anotherPropertyName}' is null.";
    }

    public static string UserNotFound()
    {
        return "User not found.";
    }

    public static string InvalidRole(string invalidRole)
    {
        return $"'{invalidRole}' is not a valid role.";
    }

    public static string FailedToHandleRole(string role, bool add, IEnumerable<string>? resultErrors)
    {
        return $"Failed to {(add ? "add" : "remove")} {role} {(add ? "to" : "remove")} role.{(resultErrors == null ? "" : $" Result Errors: {string.Join(", ", resultErrors)}")}";
    }

    public static string FailedToCreateUser(string email, IEnumerable<string>? resultErrors)
    {
        return $"Failed to create user with '{email}' email.{(resultErrors == null ? "" : $"Result Errors: {string.Join(", ", resultErrors)}")}";
    }

    public static string PropertyIsAlreadyInUse(string property)
    {
        return $"{property} is already in use.";
    }

    public static string FailedtoGeneratePasswordResetToken()
    {
        return "Failed to generate password reset token.";
    }

    public static string InvalidCredentials()
    {
        return "Invalid email or password";
    }

    public static string AuthenticatedUserNotFound()
    {
        return "Authenticated user not found.";
    }
}
