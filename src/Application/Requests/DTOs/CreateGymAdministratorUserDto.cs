namespace Fitpass.Application.Requests.DTOs;

public class CreateGymAdministratorUserDto
{
    public required string GymId { get; set; }
    public required string GymAdminFirstName { get; set; }
    public required string GymAdminLastName { get; set; }
    public required string GymAdminEmail { get; set; }
    public required string GymAdminPassword { get; set; }
    public required string EscalationEmail { get; set; }
}