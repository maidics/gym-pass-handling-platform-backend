using FitPass.Application.GymMembershipPasses.DTOs;
using FitPass.Application.UserProfiles.DTOs;
using FitPass.Domain.Enums;

namespace FitPass.Application.GymMemberships.DTOs;

public class GymMembershipWithUserProfileAndEmailDto
{
    public required string Id { get; set; }
    public required string UserId { get; set; }
    public required string GymId { get; set; }
    public required GymMembershipStatus Status { get; set; }
    public required UserProfileWithEmailDto UserProfile { get; set; }
}
