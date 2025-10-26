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

    public static string InvalidEmailAddress()
    {
        return "Provided email address is invalid.";
    }

    public static string InvalidPhoneNumber()
    {
        return "Phone number, it must contain 7 to 15 digits, with an optional '+' prefix and it must not contain any spaces or other special characters.";
    }

    public static string PropertyMustEqualToAnotherProperty(string property1Name, string property2Name)
    {
        return $"'{property1Name}' and '{property2Name}' must be the same";
    }
}
