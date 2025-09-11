namespace FitPass.Domain.Entities;

public class GymStaffAssigment
{
    public required string ApplicationUserId { get; set; }
    public required string GymId { get; set; }
    public required Gym Gym { get; set; }
    public required string Role { get; set; }
    public required string EscalationEmail { get; set; }
}