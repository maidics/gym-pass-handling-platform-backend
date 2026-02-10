using FitPass.Application.UserProfiles.DTOs;

namespace FitPass.Application.GymEmployments.DTOs;

public class GymEmploymentDto
{
    public required string Id { get; set; }
    public required string UserId { get; set; }
    public required string GymId { get; set; }
    public required string? SupervisorEmail { get; set; }
    public required string Role { get; set; }
    public required DateTimeOffset CreatedOn { get; set; }
    public required UserProfileWithEmailDto UserProfile { get; set; }
}
