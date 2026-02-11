using FitPass.Domain.ValueObjects;

namespace FitPass.Domain.Entities;

public class GymContactInfo : ContactInfoBase
{
    public required string FullName { get; set; }
    public required Address? Address { get; set; } //address in case they want to specify hq or and office
}
