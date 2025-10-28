namespace FitPass.Domain.Entities;

public class UserProfile
{
    public required string ApplicationUserId { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
}
