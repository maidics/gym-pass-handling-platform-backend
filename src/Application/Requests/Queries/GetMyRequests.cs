using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Security;
using FitPass.Application.Requests.DTOs;

namespace FitPass.Application.Requests.Queries;

[Authorize]
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
    
    public async Task<List<RequestDto>> Handle(GetMyRequestsQuery query, CancellationToken cancellationToken)
    {
        var requests = await _context.Requests
            .AsNoTracking()
            .Where(x => x.CreatedBy == _user.Id)
            .ToListAsync(cancellationToken);

        return requests.Select(r => r.MapToDto()).ToList();
    }
}
