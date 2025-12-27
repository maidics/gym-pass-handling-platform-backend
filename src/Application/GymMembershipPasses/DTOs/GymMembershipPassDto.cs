using FitPass.Domain.Entities;
using FitPass.Domain.Enums;

namespace FitPass.Application.GymMembershipPasses.DTOs;

public class GymMembershipPassDto
{
    public required string Id { get; set; }
    public required string GymMembershipId { get; set; }
    public required PassType Type { get; set; }
    public required int? TotalUses { get; set; }
    public required int? RemainingUses { get; set; }
    public required DateTimeOffset? ExpirationDate { get; set; }
}

public static class Mappings
{
    extension(GymMembershipPass gymMembershipPass)
    {
        public GymMembershipPassDto MapToDto()
        {
            return new GymMembershipPassDto
            {
                Id = gymMembershipPass.Id,
                GymMembershipId = gymMembershipPass.GymMembershipId,
                Type = gymMembershipPass.Type,
                TotalUses = gymMembershipPass.TotalUses,
                RemainingUses = gymMembershipPass.RemainingUses,
                ExpirationDate = gymMembershipPass.ExpirationDate
            };
        }
    }
}
