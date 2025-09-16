using FitPass.Domain.Entities;
using FitPass.Domain.Enums;

namespace Fitpass.Application.Requests.DTOs;

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
    public required RequestType RequestType { get; set; }
    public required RequestStatus RequestStatus { get; set; }
    public required string? RequestPayload { get; set; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Request, RequestDto>();
        }
    }
}