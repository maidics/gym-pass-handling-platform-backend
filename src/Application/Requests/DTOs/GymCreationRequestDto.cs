using FitPass.Domain.Enums;

namespace FitPass.Application.Requests.DTOs;
public class GymCreationRequestDto
{
    public required string Id { get; set; }
    public required string Title { get; set; }
    public required string Description { get; set; }
    public required PriorityLevel PriorityLevel { get; set; }
    public required RequestStatus RequestStatus { get; set; }
    public required CreateGymDTO CreateGymDTO { get; set; }
}
