using FitPass.Application.Requests.DTOs;
using FitPass.Application.Common.Extensions;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Security;
using FitPass.Domain.Constants;

namespace FitPass.Application.Requests.Queries;

[Authorize(Roles = Roles.AppAdministrator)]
public record GetRequestQuery(string RequestId) : IRequest<RequestDto>;

public class GetRequestQueryValidator : AbstractValidator<GetRequestQuery>
{
    public GetRequestQueryValidator()
    {
        RuleFor(v => v.RequestId).NotEmptyWithMessage(nameof(GetRequestQuery.RequestId));
    }
}

public class GetRequestQueryHandler : IRequestHandler<GetRequestQuery, RequestDto>
{
    private readonly IApplicationDbContext _context;

    public GetRequestQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }
    public async Task<RequestDto> Handle(GetRequestQuery query, CancellationToken cancellationToken)
    {
        var gymCreationRequest = await _context.Requests.AsNoTracking().FirstOrDefaultAsync(gcr => gcr.Id == query.RequestId, cancellationToken);

        Guard.Against.NotFound(query.RequestId, gymCreationRequest, "Id");

        return gymCreationRequest.MapToDto();
    }
}
