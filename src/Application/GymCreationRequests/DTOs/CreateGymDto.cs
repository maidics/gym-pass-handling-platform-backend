namespace FitPass.Application.Requests.DTOs;

public class CreateGymDTO
{
    public required string GymName { get; set; }
    public required string GymAddress { get; set; }
    public string GymOwnerName { get; set; } = string.Empty;
    public required string GymAdminEmail { get; set; }
    public required string GymAdminFirstName{ get; set; }
    public required string GymAdminLastName{ get; set; }
    public required string GymAdminPassword { get; set; }
    public required string GymAdminPasswordConfirm { get; set; }
    public required string EscalationEmail { get; set; }
}