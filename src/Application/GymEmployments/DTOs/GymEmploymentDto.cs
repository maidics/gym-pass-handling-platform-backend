using FitPass.Application.UserProfiles.DTOs;

namespace FitPass.Application.GymEmployments.DTOs;

public class GymEmploymentDto
{
    public required string? ApplicationUserId { get; set; }
    public required string? GymId { get; set; }
    public required string? EscalationEmail { get; set; }
    public required string Role { get; set; }
    public DateTimeOffset EmploymentStart = DateTimeOffset.UtcNow;
    public DateTimeOffset? EmploymentEnd = null;
    public required UserProfileWithEmailDto UserProfile { get; set; }
}
