using FitPass.Application.GymMembershipPasses.DTOs;
using FitPass.Domain.Entities;
using FitPass.Domain.Enums;

namespace FitPass.Application.GymMemberships.DTOs;

public class GymMembershipDto
{
    public required string? ApplicationUserId { get; set; }
    public required string GymId { get; set; }
    public required GymMembershipStatus GymMembershipStatus { get; set; }
    public required DateTimeOffset? MemberSince { get; set; }
    public required List<GymMembershipPassDto> OwnedPasses { get; set; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<GymMembership, GymMembershipDto>();
        }
    }
}
