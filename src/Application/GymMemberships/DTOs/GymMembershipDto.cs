using Fitpass.Application.Gyms.DTOs;
using FitPass.Application.OwnedPasses.DTOs;
using FitPass.Domain.Entities;
using FitPass.Domain.Enums;

namespace FitPass.Application.GymMemberships.DTOs;

public class GymMembershipDto
{
    public required string? ApplicationUserId { get; set; }
    public required string? NonRegisteredUserId { get; set; }
    public required string GymId { get; set; }
    public required GymMembershipStatus GymMembershipStatus { get; set; }
    public required DateTimeOffset? MemberSince { get; set; }
    public required GymDto Gym { get; set; }
    public required List<OwnedPassDto> OwnedPasses { get; set; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<GymMembership, GymMembershipDto>();
        }
    }
}
