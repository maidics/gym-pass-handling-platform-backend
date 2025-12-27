using FitPass.Application.Gyms.DTOs;
using FitPass.Application.Common.Extensions;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Security;
using FitPass.Domain.Constants;
using FitPass.Application.Common.Models;
using FitPass.Domain.Entities;
using FitPass.Application.Common.Resources;

namespace FitPass.Application.Gyms.Queries;

[Authorize(Roles = $"{Roles.AppAdministrator}")]
public record GetGymByIdQuery(string GymId) : IRequest<Result<GymDto>>;

public class GetGymByIdQueryValidator : AbstractValidator<GetGymByIdQuery>
{
    public GetGymByIdQueryValidator(ILocalizer localizer)
    {
        RuleFor(v => v.GymId)
            .PropertyOfEntityNotEmptyWithMessageLocalized(localizer, nameof(SharedResource.Id), nameof(SharedResource.Gym));
    }
}

public class GetGymByIdQueryHandler : IRequestHandler<GetGymByIdQuery, Result<GymDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ILocalizer _localizer;

    public GetGymByIdQueryHandler(
        IApplicationDbContext context,
        ILocalizer localizer)
    {
        _context = context;
        _localizer = localizer;
    }

    public async Task<Result<GymDto>> Handle(GetGymByIdQuery query, CancellationToken cancellationToken)
    {
        var gym = await _context
            .Gyms
            .AsNoTracking()
            .FirstOrDefaultAsync(g => g.Id == query.GymId, cancellationToken);

        if (gym is null)
        {
            return Result.NotFound(_localizer.GetNotFound(nameof(SharedResource.Gym)));
        }

        return Result.Success(gym.MapToDto());
    }
}
