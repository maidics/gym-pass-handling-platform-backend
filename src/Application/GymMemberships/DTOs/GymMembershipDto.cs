using FitPass.Application.GymMembershipPasses.DTOs;
using FitPass.Domain.Entities;
using FitPass.Domain.Enums;

namespace FitPass.Application.GymMemberships.DTOs;

public class GymMembershipDto
{
    public required string Id { get; set; }
    public required string? ApplicationUserId { get; set; }
    public required string GymId { get; set; }
    public required GymMembershipStatus Status { get; set; }
    public required DateTimeOffset? CreatedOn { get; set; }
    public required string? CreatedBy { get; set; }
    public required List<GymMembershipPassDto> Passes { get; set; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<GymMembership, GymMembershipDto>();
        }
    }
}
