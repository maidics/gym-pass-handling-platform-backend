using FitPass.Application.Requests.DTOs;
using FitPass.Application.Common.Extensions;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Security;
using FitPass.Domain.Constants;
using FitPass.Application.Common.Models;
using FitPass.Domain.Entities;

namespace FitPass.Application.Requests.Queries;

[Authorize(Roles = Roles.AppAdministrator)]
public record GetRequestQuery(string RequestId) : IRequest<Result<RequestDto>>;

public class GetRequestQueryValidator : AbstractValidator<GetRequestQuery>
{
    public GetRequestQueryValidator()
    {
        RuleFor(v => v.RequestId).NotEmptyWithMessage(nameof(GetRequestQuery.RequestId));
    }
}

public class GetRequestQueryHandler : IRequestHandler<GetRequestQuery, Result<RequestDto>>
{
    private readonly IApplicationDbContext _context;

    public GetRequestQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }
    public async Task<Result<RequestDto>> Handle(GetRequestQuery query, CancellationToken cancellationToken)
    {
        var request = await _context.Requests.AsNoTracking().FirstOrDefaultAsync(gcr => gcr.Id == query.RequestId, cancellationToken);

        if (request is null)
        {
            return Result.NotFound(nameof(Request));
        }

        return Result.Success(request.MapToDto());
    }
}
