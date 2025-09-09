namespace FitPass.Domain.Entities;

public class UserGymMembership : BaseEntity
{
    public required string ApplicationUserId { get; set; }
    public required string GymId { get; set; }
    public GymMembershipStatus GymMembershipStatus { get; set; } = GymMembershipStatus.Member;
    public DateTimeOffset? MemberSince { get; set; } = DateTimeOffset.UtcNow;
    public required Gym Gym { get; set; }
    public ICollection<OwnedPass> OwnedPasses { get; set; } = [];
}