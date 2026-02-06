using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Security;
using FitPass.Application.Requests.DTOs;
using FitPass.Domain.Constants;

namespace FitPass.Application.Requests.Queries;

[Authorize(
    Roles = $"{Roles.User},{Roles.PendingGymEmployee},{Roles.GymStaff},{Roles.GymAdministrator}"
)]
public record GetMyRequestsQuery : IRequest<List<RequestDto>>;

public class GetMyRequestsQueryHandler : IRequestHandler<GetMyRequestsQuery, List<RequestDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public GetMyRequestsQueryHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task<List<RequestDto>> Handle(
        GetMyRequestsQuery query,
        CancellationToken cancellationToken
    )
    {
        return await _context
            .Requests.AsNoTracking()
            .Where(x => x.CreatedBy == _user.Id)
            .Select(x => new RequestDto()
            {
                Id = x.Id,
                CreatedBy = x.CreatedBy,
                CreatedOn = x.CreatedOn,
                Description = x.Description,
                Title = x.Title,
                Error = x.Error,
                HandlerRationale = x.HandlerRationale,
                LastModifiedBy = x.LastModifiedBy,
                LastModifiedOn = x.LastModifiedOn,
                Payload = x.Payload,
                PriorityLevel = x.PriorityLevel,
                Status = x.Status,
                Type = x.Type,
            })
            .ToListAsync(cancellationToken);
    }
}
