namespace FitPass.Application.UserProfiles.DTOs;

public class UserProfileWithEmailDto
{
    public required string ApplicationUserId { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string? Email { get; set; }
}
