namespace FitPass.Domain.Entities;

public class UserGymMembership : BaseEntity
{
    public required string UserId { get; set; }
    public required string? GymId { get; set; }
    public GymMembershipStatus GymMembershipStatus { get; set; } = GymMembershipStatus.Member;
    public DateTimeOffset? MemberSince { get; set; } = DateTimeOffset.UtcNow;
    public ApplicationUser ApplicationUser { get; set; } = null!;
    public Gym Gym { get; set; } = null!;
    public ICollection<OwnedPass> OwnedPasses { get; set; } = [];
}
