using FitPass.Domain.ValueObjects;

namespace FitPass.Domain.Common;

public abstract class ContactInfoBase : BaseEntity
{
    public PhoneNumber? PhoneNumber { get; set; }
    public string? Email { get; set; }
}
