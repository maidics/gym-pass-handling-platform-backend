namespace FitPass.Domain.Entities;

public class GymMembership : BaseEntity
{
    public required string? ApplicationUserId { get; set; }
    public required string? GymId { get; set; }
    public GymMembershipStatus GymMembershipStatus { get; set; } = GymMembershipStatus.Member;
    public DateTimeOffset? MemberSince { get; set; } = DateTimeOffset.UtcNow;
    public Gym? Gym { get; set; }
    public ICollection<GymMembershipPass> Passes { get; set; } = [];
}