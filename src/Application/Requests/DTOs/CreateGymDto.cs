using FitPass.Domain.Enums;
using FitPass.Domain.ValueObjects;

namespace FitPass.Application.Requests.DTOs;

public class CreateGymDto
{
    public required string GymName { get; set; }
    public required Address GymAddress { get; set; }
    public required GymStatus GymStatus { get; set; }
    public required GymTier GymTier { get; set; }
    public string GymOwnerName { get; set; } = string.Empty;
    public required string EscalationEmail { get; set; }
}
