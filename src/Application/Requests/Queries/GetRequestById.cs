using FitPass.Application.Requests.DTOs;
using FitPass.Application.Common.Extensions;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Security;
using FitPass.Domain.Constants;
using FitPass.Application.Common.Models;
using FitPass.Domain.Entities;

namespace FitPass.Application.Requests.Queries;

[Authorize(Roles = Roles.AppAdministrator)]
public record GetRequestByIdQuery(string RequestId) : IRequest<Result<RequestDto>>;

public class GetRequestByIdQueryValidator : AbstractValidator<GetRequestByIdQuery>
{
    public GetRequestByIdQueryValidator()
    {
        RuleFor(v => v.RequestId).NotEmptyWithMessage(nameof(GetRequestByIdQuery.RequestId));
    }
}

public class GetRequestByIdQueryHandler : IRequestHandler<GetRequestByIdQuery, Result<RequestDto>>
{
    private readonly IApplicationDbContext _context;

    public GetRequestByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }
    public async Task<Result<RequestDto>> Handle(GetRequestByIdQuery query, CancellationToken cancellationToken)
    {
        var request = await _context.Requests.AsNoTracking().FirstOrDefaultAsync(gcr => gcr.Id == query.RequestId, cancellationToken);

        if (request is null)
        {
            return Result.NotFound(nameof(Request));
        }

        return Result.Success(request.MapToDto());
    }
}
