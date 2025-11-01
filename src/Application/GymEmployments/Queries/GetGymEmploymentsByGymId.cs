using FitPass.Application.Common.Extensions;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Security;
using FitPass.Application.GymEmployments.DTOs;
using FitPass.Domain.Constants;

namespace Fitpass.Application.GymEmployments.Queries;

[Authorize(Roles = Roles.AppAdministrator)]
public record GetGymEmploymentsByGymIdQuery(string GymId) : IRequest<List<GymEmploymentDto>>;

public class GetGymEmploymentsByGymIdQueryValidator : AbstractValidator<GetGymEmploymentsByGymIdQuery>
{
    public GetGymEmploymentsByGymIdQueryValidator()
    {
        RuleFor(v => v.GymId).NotEmptyWithMessage(nameof(GetGymEmploymentsByGymIdQuery.GymId));
    }
}

public class GetGymEmploymentsByGymIdQueryHandler : IRequestHandler<GetGymEmploymentsByGymIdQuery, List<GymEmploymentDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IQueryService _queryService;    

    public GetGymEmploymentsByGymIdQueryHandler(IApplicationDbContext context, IQueryService queryService)
    {
        _context = context;
        _queryService = queryService;
    }

    public async Task<List<GymEmploymentDto>> Handle(GetGymEmploymentsByGymIdQuery query, CancellationToken cancellationToken)
    {
        var gym = await _context.Gyms.AsNoTracking().FirstOrDefaultAsync(g => g.Id == query.GymId, cancellationToken);

        Guard.Against.NotFound(query.GymId, gym, "GymId");

        return await _queryService.GetGymEmploymentsWithUserProfileAndEmailByGymId(gym.Id);
    }
}
