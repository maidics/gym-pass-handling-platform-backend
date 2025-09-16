using FitPass.Domain.Entities;

namespace FitPass.Application.NonRegisteredUsers.DTOs;
public class NonRegisteredUserDto
{
    public required string? Email { get; set; }
    public required string? PhoneNumber { get; set; }
    public required string FirstName { get; set; }
    public required string? LastName { get; set; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<NonRegisteredUser, NonRegisteredUserDto>();
        }
    }
}
