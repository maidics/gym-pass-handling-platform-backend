using FitPass.Application.Requests.DTOs;
using FitPass.Application.Common.Extensions;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Security;
using FitPass.Domain.Constants;
using FitPass.Domain.Enums;

namespace FitPass.Application.Requests.Queries;

[Authorize(Roles = Roles.AppAdministrator)]
public record GetRequestsQuery(RequestType? RequestType, RequestStatus? RequestStatus) : IRequest<List<RequestDto>>;

public class GetRequestsQueryValidator : AbstractValidator<GetRequestsQuery>
{
    public GetRequestsQueryValidator()
    {
        When(v => v.RequestType != null, () =>
        {
            RuleFor(v => (RequestType)v.RequestType!).NotEmptyWithMessage(nameof(GetRequestsQuery.RequestType));
        });

        When(v => v.RequestStatus != null, () =>
        {
            RuleFor(v => (RequestStatus)v.RequestStatus!).NotEmptyWithMessage(nameof(GetRequestsQuery.RequestStatus));
        });
    }
}

public class GetRequestsQueryHandler : IRequestHandler<GetRequestsQuery, List<RequestDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetRequestsQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }
    public async Task<List<RequestDto>> Handle(GetRequestsQuery query, CancellationToken cancellationToken)
    {
        var requestsQuery = _context.Requests.AsNoTracking().AsQueryable();

        if (query.RequestStatus != null)
        {
            requestsQuery = requestsQuery.Where(r => r.Status == query.RequestStatus);
        }

        if (query.RequestType != null)
        {
            requestsQuery = requestsQuery.Where(r => r.Type == query.RequestType);
        }

        return await requestsQuery.ProjectTo<RequestDto>(_mapper.ConfigurationProvider).ToListAsync(cancellationToken);
    }
}
