using FitPass.Domain.Entities;

namespace Fitpass.Application.Requests.DTOs;

public class GymAdminNominationDto
{
    public required string GymId { get; set; }
    public required string UserIdToNominate { get; set; }
    public required string EscalationEmail { get; set; }
}