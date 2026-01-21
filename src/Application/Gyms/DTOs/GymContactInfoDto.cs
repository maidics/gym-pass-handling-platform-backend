using FitPass.Domain.Common;
using FitPass.Domain.Entities.ContactInfos;
using FitPass.Domain.ValueObjects;

namespace FitPass.Application.Gyms.DTOs;

public class GymContactInfoDto
{
    public required string FullName {  get; set; }
    public required Address? Address { get; set; }
    public required PhoneNumber? PhoneNumber { get; set; }
    public required string? Email { get; set; }
}

public static partial class Mappings
{
    extension(GymContactInfo contactInfo)
    {
        public GymContactInfoDto MapToDto()
        {
            return new GymContactInfoDto
            {
                FullName = contactInfo.FullName, 
                Address = contactInfo.Address, 
                Email = contactInfo.Email,  
                PhoneNumber = contactInfo.PhoneNumber
            };
        }
    }
}
