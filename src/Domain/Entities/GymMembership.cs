namespace FitPass.Domain.Entities;

public class GymMembership : BaseAuditableEntity
{
    public required string ApplicationUserId { get; set; }
    public required string? GymId { get; set; }
    public GymMembershipStatus Status { get; set; } = GymMembershipStatus.Member;
    public Gym? Gym { get; set; }
    public ICollection<GymMembershipPass> Passes { get; set; } = [];
}
