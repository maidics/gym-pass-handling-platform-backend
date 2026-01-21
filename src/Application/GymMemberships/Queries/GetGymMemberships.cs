using FitPass.Application.Common.Extensions;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Security;
using FitPass.Application.GymMemberships.DTOs;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using FitPass.Domain.Enums;

namespace FitPass.Application.GymMemberships.Queries;

[Authorize(Roles = $"{Roles.GymAdministrator},{Roles.GymStaff}")]
public record GetGymMembershipsToMyGymQuery
    : IRequest<List<GymMembershipWithUserProfileAndEmailDto>>;

public class GetGymMembershipsToMyGymQueryHandler : IRequestHandler<GetGymMembershipsToMyGymQuery, List<GymMembershipWithUserProfileAndEmailDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly IQueryService _queryService;


    public GetGymMembershipsToMyGymQueryHandler(
        IApplicationDbContext context,
        IUser user,
        IQueryService queryService
    )
    {
        _context = context;
        _queryService = queryService;
        _user = user;
    }

    public async Task<List<GymMembershipWithUserProfileAndEmailDto>> Handle(GetGymMembershipsToMyGymQuery query, CancellationToken cancellationToken)
    {
        var gymEmployment = await _context
            .GymEmployments
            .AsNoTracking()
            .FirstOrDefaultAsync(ge => ge.UserId == _user.Id, cancellationToken);

        Guard.Against.NullParameterRelatedToCurrentUser(gymEmployment, nameof(GymEmployment), _user.Id);

        return await _queryService.GetGymMembershipsWithUserProfilesAndEmailByGymIdAndMembershipStatus(gymEmployment.GymId!, null);
    }
}
