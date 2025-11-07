namespace FitPass.Domain.Entities;

public class GymPassUsage : BaseAuditableEntity
{
    public required string ApplicationUserId { get; init; }
    public required string GymMembershipPassId { get; init; }
    public GymMembershipPass Pass { get; init; } = null!;
}
