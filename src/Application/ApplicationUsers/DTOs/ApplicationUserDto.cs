using FitPass.Application.GymStaffAssignments.DTOs;
using FitPass.Application.UserGymMemberships.DTOs;
using FitPass.Domain.Entities;

namespace FitPass.Application.ApplicationUsers.DTOs;

public class ApplicationUserDto
{
    public required string Id { get; set; }
    public required string Email { get; set; }
    public required string FirstName { get; set; }
    public required string? LastName { get; set; }
    public required List<UserGymMembershipDto>? UserGymMemberships { get; set; }
    public required GymStaffAssignmentDto? GymStaffAssigment { get; set; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<ApplicationUser, ApplicationUserDto>();
        }
    }
}
