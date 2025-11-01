using FitPass.Domain.Entities;
using FitPass.Domain.Enums;

namespace FitPass.Application.GymMembershipPasses.DTOs;

public class GymMembershipPassDto
{
    public required string Id { get; set; }
    public required string? UserGymMembershipId { get; set; }
    public required PassType Type { get; set; }
    public required int? TotalUses { get; set; }
    public required int? RemainingUses { get; set; }
    public required DateOnly? ExpirationDate { get; set; }
    public required decimal EurPrice { get; set; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<GymMembershipPass, GymMembershipPassDto>();
        }
    }
}
