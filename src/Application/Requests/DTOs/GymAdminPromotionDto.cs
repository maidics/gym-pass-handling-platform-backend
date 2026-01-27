namespace FitPass.Application.Requests.DTOs;

public class GymAdminPromotionDto
{
    public required string GymId { get; set; }
    public required string PendingGymEmployeeEmail { get; set; }
    public required string SupervisorEmail { get; set; }
}
