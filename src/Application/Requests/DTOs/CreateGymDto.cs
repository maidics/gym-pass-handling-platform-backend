using FitPass.Domain.Enums;
using FitPass.Domain.ValueObjects;

namespace FitPass.Application.Requests.DTOs;

public class CreateGymDto
{
    public required string Name { get; set; }
    public required Address Address { get; set; }
    public required GymStatus Status { get; set; }
    public required GymTier Tier { get; set; }
    public required string EscalationEmail { get; set; }
}
