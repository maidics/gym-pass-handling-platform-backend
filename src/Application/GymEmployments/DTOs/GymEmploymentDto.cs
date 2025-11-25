using FitPass.Application.UserProfiles.DTOs;

namespace FitPass.Application.GymEmployments.DTOs;

public class GymEmploymentDto
{
    public required string? ApplicationUserId { get; set; }
    public required string? GymId { get; set; }
    public required string? EscalationEmail { get; set; }
    public required string Role { get; set; }
    public required DateTimeOffset EmploymentStart { get; set; }
    public DateTimeOffset? EmploymentEnd = null;
    public required UserProfileWithEmailDto UserProfile { get; set; }
}
