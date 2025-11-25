namespace FitPass.Domain.Entities;

public class GymEmployment : BaseEntity
{
    public required string UserId { get; set; } //User's id is detached when employment ends => record is still in db
    public required string GymId { get; set; }
    public string? EscalationEmail { get; set; }
    public required string Role { get; set; }
    public required DateTimeOffset EmploymentStart { get; set; }
    public DateTimeOffset? EmploymentEnd { get; set; } = null;
    public Gym? Gym { get; set; }
}
