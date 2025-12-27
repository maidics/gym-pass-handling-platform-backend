using FitPass.Application.Requests.DTOs;
using FitPass.Application.Common.Extensions;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Security;
using FitPass.Domain.Constants;
using FitPass.Application.Common.Models;
using FitPass.Domain.Entities;
using FitPass.Application.Common.Resources;

namespace FitPass.Application.Requests.Queries;

[Authorize(Roles = Roles.AppAdministrator)]
public record GetRequestByIdQuery(string RequestId) : IRequest<Result<RequestDto>>;

public class GetRequestByIdQueryValidator : AbstractValidator<GetRequestByIdQuery>
{
    public GetRequestByIdQueryValidator(ILocalizer localizer)
    {
        RuleFor(v => v.RequestId)
            .PropertyOfEntityNotEmptyWithMessageLocalized(localizer, nameof(SharedResource.Id), nameof(SharedResource.Request));
    }
}

public class GetRequestByIdQueryHandler : IRequestHandler<GetRequestByIdQuery, Result<RequestDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ILocalizer _localizer;

    public GetRequestByIdQueryHandler(IApplicationDbContext context, ILocalizer localizer)
    {
        _context = context;
        _localizer = localizer;
    }
    public async Task<Result<RequestDto>> Handle(GetRequestByIdQuery query, CancellationToken cancellationToken)
    {
        var request = await _context.Requests.AsNoTracking().FirstOrDefaultAsync(gcr => gcr.Id == query.RequestId, cancellationToken);

        if (request is null)
        {
            return Result.NotFound(_localizer.GetNotFound(nameof(SharedResource.Request)));
        }

        return Result.Success(request.MapToDto());
    }
}
