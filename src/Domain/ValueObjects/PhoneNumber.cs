using System.Text.RegularExpressions;

namespace FitPass.Domain.ValueObjects;

public partial class PhoneNumber : ValueObject
{
    [GeneratedRegex(@"^\+[1-9]\d{1,14}$")]
    private static partial Regex E164Regex();
    
    public string Value { get; private set; }
    
    private PhoneNumber(string value)
    {
        Value = value;
    }
    
    public static PhoneNumber Create(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
        {
            throw new ArgumentException("Phone number cannot be empty.", nameof(phoneNumber));
        }
        
        var normalized = Normalize(phoneNumber);
        
        if (!E164Regex().IsMatch(normalized))
        {
            throw new ArgumentException(
                "Phone number must contain 7-15 digits and include country code (e.g., +1234567890).", 
                nameof(phoneNumber));
        }
        
        return new PhoneNumber(normalized);
    }

    public static bool IsValid(string phoneNumber)
    {
        return E164Regex().IsMatch(phoneNumber);
    }
    
    private static string Normalize(string phoneNumber)
    {
        var cleaned = phoneNumber.Trim();
        var hasPlus = cleaned.StartsWith('+');
        
        var digitsOnly = Regex.Replace(cleaned, @"\D", "");
        
        if (string.IsNullOrEmpty(digitsOnly))
        {
            throw new ArgumentException("Phone number must contain digits.", nameof(phoneNumber));
        }
        
        return hasPlus || digitsOnly.Length > 10 
            ? $"+{digitsOnly}" 
            : $"+{digitsOnly}";
    }
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
    
    public override string ToString() => Value;
    
    public static implicit operator string(PhoneNumber phoneNumber) => phoneNumber.Value;
}
