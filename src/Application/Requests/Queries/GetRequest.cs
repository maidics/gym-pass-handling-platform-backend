using Fitpass.Application.Requests.DTOs;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Security;
using FitPass.Application.Extensions;
using FitPass.Domain.Constants;

namespace Fitpass.Application.Requests.Queries;

[Authorize(Roles = Roles.AppAdministrator)]
public record GetRequestQuery(string RequestId) : IRequest<RequestDto>;

public class GetRequestQueryValidator : AbstractValidator<GetRequestQuery>
{
    public GetRequestQueryValidator()
    {
        RuleFor(v => v.RequestId).NotEmptyWithMessage("Gym creation request id");
    }
}

public class GetRequestQueryHandler : IRequestHandler<GetRequestQuery, RequestDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetRequestQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }
    public async Task<RequestDto> Handle(GetRequestQuery query, CancellationToken cancellationToken)
    {
        var gymCreationRequest = await _context.Requests.AsNoTracking().FirstOrDefaultAsync(gcr => gcr.Id == query.RequestId, cancellationToken);

        Guard.Against.NotFound(query.RequestId, gymCreationRequest, "Id");

        return _mapper.Map<RequestDto>(gymCreationRequest);
    }
}