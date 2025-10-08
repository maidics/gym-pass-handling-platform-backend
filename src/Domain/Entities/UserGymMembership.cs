namespace FitPass.Domain.Entities;

public class UserGymMembership : BaseEntity
{
    public required string? ApplicationUserId { get; set; }
    public required string? NonRegisteredUserId { get; set; }
    public required string? GymId { get; set; }
    public GymMembershipStatus GymMembershipStatus { get; set; } = GymMembershipStatus.Member;
    public DateTimeOffset? MemberSince { get; set; } = DateTimeOffset.UtcNow;
    public ApplicationUser? ApplicationUser { get; set; }
    public NonRegisteredUser? NonRegisteredUser { get; set; }
    public Gym Gym { get; set; } = null!;
    public ICollection<OwnedPass> OwnedPasses { get; set; } = [];
}
