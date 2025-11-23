using FitPass.Domain.Entities;
using FitPass.Domain.Enums;

namespace FitPass.Application.Requests.DTOs;

public class RequestDto
{
    public required string Id { get; set; }
    public required DateTimeOffset CreatedOn { get; set; }
    public required string? CreatedBy { get; set; }
    public required DateTimeOffset LastModifiedOn { get; set; }
    public required string? LastModifiedBy { get; set; }
    public required string Title { get; set; }
    public required string Description { get; set; }
    public required PriorityLevel PriorityLevel { get; set; }
    public required RequestType Type { get; set; }
    public required RequestStatus Status { get; set; }
    public string? Payload { get; set; }
}

public static partial class Mappings
{
    extension(Request request)
    {
        public RequestDto MapToDto()
        {
            return new RequestDto
            {
                Id = request.Id,
                CreatedOn = request.CreatedOn,
                CreatedBy = request.CreatedBy,
                LastModifiedOn = request.LastModifiedOn,
                LastModifiedBy = request.LastModifiedBy,
                Title = request.Title,
                Description = request.Description,
                PriorityLevel = request.PriorityLevel,
                Type = request.Type,
                Status = request.Status,
                //Payload = request.Payload
            };
        }
    }
}
