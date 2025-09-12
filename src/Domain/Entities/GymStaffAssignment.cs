namespace FitPass.Domain.Entities;

public class GymStaffAssigment
{
    public required string ApplicationUserId { get; set; }
    public required string GymId { get; set; }
    public required string EscalationEmail { get; set; }
    public ApplicationUser ApplicationUser { get; set; } = null!;
    public Gym Gym { get; set; } = null!;
}