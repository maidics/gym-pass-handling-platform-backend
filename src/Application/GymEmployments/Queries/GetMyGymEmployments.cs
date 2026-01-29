using FitPass.Application.Common.Extensions;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Security;
using FitPass.Application.GymEmployments.DTOs;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;

namespace FitPass.Application.GymEmployments.Queries;

[Authorize(Roles = $"{Roles.GymAdministrator},{Roles.GymStaff}")]
public record GetMyGymEmploymentsQuery : IRequest<List<GymEmploymentDto>>;

public class GetMyGymEmploymentsQueryHandler : IRequestHandler<GetMyGymEmploymentsQuery, List<GymEmploymentDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly IQueryService _queryService;

    public GetMyGymEmploymentsQueryHandler(
        IApplicationDbContext context, 
        IUser user, 
        IQueryService queryService)
    {
        _context = context;
        _user = user;
        _queryService = queryService;
    }
    public async Task<List<GymEmploymentDto>> Handle(GetMyGymEmploymentsQuery query, CancellationToken cancellationToken)
    {
        var gymEmployment = await _context.GymEmployments
            .AsNoTracking()
            .FirstOrDefaultAsync(ge => ge.UserId == _user.Id, cancellationToken);

        Guard.Against.NullParameterRelatedToCurrentUser(gymEmployment, nameof(GymEmployment), _user.Id);

        return await _queryService.GetGymEmploymentsWithUserProfileAndEmailByGymId(gymEmployment.GymId!, cancellationToken);
    }
}
