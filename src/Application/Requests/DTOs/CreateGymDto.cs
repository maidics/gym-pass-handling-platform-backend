using FitPass.Domain.Enums;

namespace FitPass.Application.Requests.DTOs;

public class CreateGymDto
{
    public required string GymName { get; set; }
    public required string GymAddress { get; set; }
    public required GymStatus GymStatus { get; set; }
    public required GymTier GymTier { get; set; }
    public string GymOwnerName { get; set; } = string.Empty;
    public required string EscalationEmail { get; set; }
}