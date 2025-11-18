namespace FitPass.Application.Common.Exceptions;
public class ConflictException : Exception
{
    public ConflictException(string propertyName) : base($"{propertyName} is already in use.") { }
}
