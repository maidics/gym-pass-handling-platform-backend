namespace FitPass.Domain.Entities;

public class Request<T> : BaseAuditableEntity
{
    public required string Title { get; set; }
    public required string Description { get; set; }
    public required PriorityLevel PriorityLevel { get; set; }
    public required RequestType RequestType { get; set; }
    public RequestStatus RequestStatus { get; set; } = RequestStatus.Submitted;
    public required T? RequestDto { get; set; }
}