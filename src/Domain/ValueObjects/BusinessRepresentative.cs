namespace FitPass.Domain.ValueObjects;

public class BusinessRepresentative : ValueObject
{
public string FirstName { get; private init; }
    public string LastName { get; private init; }
    public string Email { get; private init; }
    public PhoneNumber Phone { get; private init; }
    public DateTime DateOfBirth { get; private init; }
    public Address Address { get; private init; }

    private BusinessRepresentative()
    {
        FirstName = string.Empty;
        LastName = string.Empty;
        Email = string.Empty;
        Phone = null!;
        Address = null!;
    }

    public BusinessRepresentative(
        string firstName,
        string lastName,
        string email,
        PhoneNumber phoneNumber,
        DateTime dateOfBirth,
        Address address,
        DateTimeOffset utcNow)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name is required", nameof(firstName));
        
        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Last name is required", nameof(lastName));
        
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email is required", nameof(email));
        
        if (phoneNumber is null)
            throw new ArgumentNullException(nameof(phoneNumber));
        
        if (dateOfBirth >= utcNow.AddYears(-18))
            throw new ArgumentException("Representative must be at least 18 years old", nameof(dateOfBirth));
        
        if (address == null)
            throw new ArgumentNullException(nameof(address));
        
        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        Email = email.Trim().ToLowerInvariant();
        Phone = phoneNumber;
        DateOfBirth = dateOfBirth.Date;
        Address = address;
    }

    public string FullName => $"{FirstName} {LastName}";

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return FirstName;
        yield return LastName;
        yield return Email;
        yield return Phone;
        yield return DateOfBirth;
        yield return Address;
    }
}
