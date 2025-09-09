namespace FitPass.Domain.Entities;

public class Request : BaseAuditableEntity
{
    public required string Title { get; set; }
    public required string Description { get; set; }
    public required PriorityLevel PriorityLevel { get; set; }
    public RequestStatus RequestStatus { get; set; } = RequestStatus.InProgress;
}