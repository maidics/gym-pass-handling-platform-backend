using FitPass.Domain.ValueObjects;

namespace FitPass.Domain.Entities.ContactInfos;

public class GymContactInfo : ContactInfoBase
{
    public required string FullName { get; set; } //TODO: add title or position in business to this
    public required Address Address { get; set; } //address in case they want to specify hq or and office
}
