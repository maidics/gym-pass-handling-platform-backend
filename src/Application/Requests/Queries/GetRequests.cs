using FitPass.Application.Requests.DTOs;
using FitPass.Application.Common.Extensions;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Security;
using FitPass.Domain.Constants;
using FitPass.Domain.Enums;

namespace FitPass.Application.Requests.Queries;

[Authorize(Roles = Roles.AppAdministrator)]
public record GetRequestsQuery : IRequest<List<RequestDto>>;

public class GetRequestsQueryHandler : IRequestHandler<GetRequestsQuery, List<RequestDto>>
{
    private readonly IApplicationDbContext _context;

    public GetRequestsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }
    public async Task<List<RequestDto>> Handle(GetRequestsQuery query, CancellationToken cancellationToken)
    {
        return await _context.Requests
            .AsNoTracking()
            .Select(x => x.MapToDto())
            .ToListAsync(cancellationToken);
    }
}
