using FitPass.Domain.Entities;

namespace Fitpass.Application.ApplicationUsers.DTOs;

public class ApplicationUserProfileDataDto
{
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<ApplicationUser, ApplicationUserProfileDataDto>();
        }
    }
}