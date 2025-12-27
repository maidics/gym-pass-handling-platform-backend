using FitPass.Application.Common.Extensions;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Models;
using FitPass.Application.Common.Resources;
using FitPass.Application.Common.Security;
using FitPass.Application.GymEmployments.DTOs;
using FitPass.Domain.Constants;

namespace FitPass.Application.GymEmployments.Queries;

[Authorize(Roles = Roles.AppAdministrator)]
public record GetGymEmploymentsByGymIdQuery(string GymId) : IRequest<Result<List<GymEmploymentDto>>>;

public class GetGymEmploymentsByGymIdQueryValidator : AbstractValidator<GetGymEmploymentsByGymIdQuery>
{
    public GetGymEmploymentsByGymIdQueryValidator(ILocalizer localizer)
    {
        RuleFor(v => v.GymId)
            .PropertyOfEntityNotEmptyWithMessageLocalized(localizer, nameof(SharedResource.Id), nameof(SharedResource.Gym));
    }
}

public class GetGymEmploymentsByGymIdQueryHandler : IRequestHandler<GetGymEmploymentsByGymIdQuery, Result<List<GymEmploymentDto>>>
{
    private readonly ILocalizer _localizer;
    private readonly IApplicationDbContext _context;
    private readonly IQueryService _queryService;

    public GetGymEmploymentsByGymIdQueryHandler(
        ILocalizer localizer,
        IApplicationDbContext context, 
        IQueryService queryService)
    {
        _localizer = localizer;
        _context = context;
        _queryService = queryService;
    }

    public async Task<Result<List<GymEmploymentDto>>> Handle(GetGymEmploymentsByGymIdQuery query, CancellationToken cancellationToken)
    {
        var gym = await _context.Gyms.AsNoTracking().FirstOrDefaultAsync(g => g.Id == query.GymId, cancellationToken);

        if (gym is null)
        {
            return Result.NotFound(_localizer.GetWithParamsLocalized(nameof(SharedResource.NotFound), nameof(SharedResource.Gym)));
        }

        return Result.Success(await _queryService.GetGymEmploymentsWithUserProfileAndEmailByGymId(gym.Id));
    }
}
