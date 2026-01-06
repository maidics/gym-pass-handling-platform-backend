namespace FitPass.Application.Requests.DTOs;

public class GymAdminPromotionDto
{
    public required string GymId { get; set; }
    public required string UserIdToNominate { get; set; }
    public required string SupervisorEmail { get; set; }
}
