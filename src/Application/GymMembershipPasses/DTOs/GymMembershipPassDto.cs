using FitPass.Domain.Entities;
using FitPass.Domain.Enums;
using FitPass.Domain.ValueObjects;

namespace FitPass.Application.GymMembershipPasses.DTOs;

public class GymMembershipPassDto
{
    public required string Id { get; set; }
    public required string? GymMembershipId { get; set; }
    public required PassType Type { get; set; }
    public required int? TotalUses { get; set; }
    public required int? RemainingUses { get; set; }
    public required DateOnly? ExpirationDate { get; set; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<GymMembershipPass, GymMembershipPassDto>();
        }
    }
}
