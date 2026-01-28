using FitPass.Domain.Entities;
using FitPass.Domain.ValueObjects;

namespace FitPass.Application.GymContactInfos.DTOs;

public class GymContactInfoDto
{
    public required string Id { get; init; }
    public required string FullName {  get; set; }
    public required Address? Address { get; set; }
    public required PhoneNumber? PhoneNumber { get; set; }
    public required string? Email { get; set; }
}

public static class Mappings
{
    extension(GymContactInfo contactInfo)
    {
        public GymContactInfoDto MapToDto()
        {
            return new GymContactInfoDto
            {
                Id =  contactInfo.Id,
                FullName = contactInfo.FullName, 
                Address = contactInfo.Address, 
                Email = contactInfo.Email,  
                PhoneNumber = contactInfo.PhoneNumber
            };
        }
    }
}
