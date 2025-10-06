namespace FitPass.Domain.Entities;

public class GymStaffAssigment
{
    public required string ApplicationUserId { get; set; }
    public string? GymId { get; set; }
    public string? EscalationEmail { get; set; }
    public required string Role { get; set; }
    public ApplicationUser ApplicationUser { get; set; } = null!;
    public Gym Gym { get; set; } = null!;
}
