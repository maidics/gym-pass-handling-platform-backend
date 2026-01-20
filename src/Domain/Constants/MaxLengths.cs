namespace FitPass.Domain.Constants;

public abstract class MaxLengths
{
    public const int Name = 100;
    public const int FullName = 200;
    public const int Email = 256;
    public const int Description = 1500;
    public const int Password = 128;
    public const int PhoneNumber = 16; //15 Numerical characters & '+'
    public const int Title = 70;
    public const int AddressLine1 = 200;
    public const int AddressLine2 = 200;
    public const int City = 100; 
    public const int State = 120;
    public const int PostalCode = 20;
    public const int CountryAlpha2 = 2;
}
