using FitPass.Domain.ValueObjects;

namespace FitPass.Domain.Entities.ContactInfos;

public class ApplicationContactInfo : ContactInfoBase
{
    public Address? Address { get; set; }
}
