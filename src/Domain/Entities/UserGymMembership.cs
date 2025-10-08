namespace FitPass.Domain.Entities;

public class UserGymMembership : BaseEntity
{
<<<<<<< HEAD
    public required string? UserId { get; set; }
    public required string? ApplicationUserId { get; set; }
=======
    public required string? ApplicationUserId { get; set; }
    public required string? NonRegisteredUserId { get; set; }
>>>>>>> 7ff3195ec35b931f759ecd737933c270b2763c2d
    public required string? GymId { get; set; }
    public GymMembershipStatus GymMembershipStatus { get; set; } = GymMembershipStatus.Member;
    public DateTimeOffset? MemberSince { get; set; } = DateTimeOffset.UtcNow;
    public ApplicationUser? ApplicationUser { get; set; }
    public NonRegisteredUser? NonRegisteredUser { get; set; }
    public Gym Gym { get; set; } = null!;
    public ICollection<OwnedPass> OwnedPasses { get; set; } = [];
}
