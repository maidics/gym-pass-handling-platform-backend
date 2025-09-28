using Fitpass.Application.Gyms.DTOs;
using FitPass.Domain.Entities;

namespace FitPass.Application.GymStaffAssignments.DTOs;

public class GymStaffAssignmentDto
{
    public required string ApplicationUserId { get; set; }
    public required string GymId { get; set; }
    public required string EscalationEmail { get; set; }
    public required string Role { get; set; }
    public GymDto Gym { get; set; } = null!;

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<GymStaffAssigment, GymStaffAssignmentDto>();
        }
    }
}
