namespace FitPass.Domain.Entities;

public class GymEmployment : BaseEntity
{
    public required string UserId { get; set; }
    public required string GymId { get; set; }
    public string? SupervisorEmail { get; set; }
    public required string Role { get; set; }
    public Gym Gym { get; set; } = null!;
}
