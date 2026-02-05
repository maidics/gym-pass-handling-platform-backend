using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Models;
using FitPass.Application.Common.Resources;
using FitPass.Application.Common.Security;
using FitPass.Application.Requests.DTOs;
using FitPass.Domain.Constants;

namespace FitPass.Application.Requests.Queries;

[Authorize(
    Roles = $"{Roles.User},{Roles.PendingGymEmployee},{Roles.GymStaff},{Roles.GymAdministrator}"
)]
public record GetMyRequestByIdQuery(string RequestId) : IRequest<Result<RequestDto>>;

public class GetMyRequestByIdQueryValidator : AbstractValidator<GetMyRequestByIdQuery>
{
    public GetMyRequestByIdQueryValidator(ILocalizer localizer)
    {
        RuleFor(v => v.RequestId)
            .NotEmpty()
            .WithMessage(
                localizer.GetPropertyOfEntityIsRequired(
                    nameof(SharedResource.Id),
                    nameof(SharedResource.Request)
                )
            );
    }
}

public class GetMyRequestByIdQueryHandler
    : IRequestHandler<GetMyRequestByIdQuery, Result<RequestDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly ILocalizer _localizer;

    public GetMyRequestByIdQueryHandler(
        IApplicationDbContext context,
        IUser user,
        ILocalizer localizer
    )
    {
        _context = context;
        _user = user;
        _localizer = localizer;
    }

    public async Task<Result<RequestDto>> Handle(
        GetMyRequestByIdQuery query,
        CancellationToken cancellationToken
    )
    {
        var dto = await _context
            .Requests.AsNoTracking()
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
            .FirstOrDefaultAsync(
                x => x.Id == query.RequestId && x.CreatedBy == _user.Id,
                cancellationToken
            );

        if (dto is null)
        {
            return Result.NotFound(_localizer.GetNotFound(nameof(SharedResource.Request)));
        }

        return Result.Success(dto);
    }
}
