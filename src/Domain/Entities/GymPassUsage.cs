namespace FitPass.Domain.Entities;

public class GymPassUsage : BaseAuditableEntity
{
    public required string ApplicationUserId { get; set; }
    public required PassType PassType { get; init; }
    public required int? TotalPassUses { get; init; }
    public required int? RemainingPassUses { get; init; }
    public required DateOnly? PassExpirationDate { get; init; }
    public required PassUseResult Result { get; init; }
    public required string GymMembershipPassId { get; init; }
    public GymMembershipPass Pass { get; init; } = null!; //TODO: test if this works with init keyword when querying
}
