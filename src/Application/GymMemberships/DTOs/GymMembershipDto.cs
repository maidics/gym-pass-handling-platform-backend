using FitPass.Application.GymMembershipPasses.DTOs;
using FitPass.Domain.Entities;
using FitPass.Domain.Enums;

namespace FitPass.Application.GymMemberships.DTOs;

public class GymMembershipDto
{
    public required string Id { get; set; }
    public required string? UserId { get; set; }
    public required string? GymId { get; set; }
    public required GymMembershipStatus Status { get; set; }
    public required DateTimeOffset? CreatedOn { get; set; }
    public required string? CreatedBy { get; set; }
    public required List<GymMembershipPassDto> Passes { get; set; }
}

public static partial class Mappings
{
    extension(GymMembership gymMembership)
    {
        public GymMembershipDto MapToDto()
        {
            return new GymMembershipDto
            {
                Id = gymMembership.Id,
                UserId = gymMembership.UserId,
                GymId = gymMembership.GymId,
                Status = gymMembership.Status,
                CreatedOn = gymMembership.CreatedOn,
                CreatedBy = gymMembership.CreatedBy,
                Passes = gymMembership.Passes.Select(p => p.MapToDto()).ToList()
            };
        }
    }
}
