namespace FitPass.Domain.Entities;

public class Request : BaseAuditableEntity
{
    public required string Title { get; set; }
    public required string Description { get; set; }
    public required PriorityLevel PriorityLevel { get; set; }
    public required RequestType Type { get; set; }
    public RequestStatus Status { get; set; } = RequestStatus.Submitted;
    public string? HandlerRationale { get; set; }
    public string? Error { get; set; }
    public required string? Payload { get; set; } //Json serialized
}
