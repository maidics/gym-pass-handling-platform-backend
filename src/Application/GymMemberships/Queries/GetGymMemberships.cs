using FitPass.Application.Common.Extensions;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Models;
using FitPass.Application.Common.Security;
using FitPass.Application.GymMemberships.DTOs;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using FitPass.Domain.Enums;

namespace FitPass.Application.GymMemberships.Queries;

[Authorize(Roles = $"{Roles.GymAdministrator},{Roles.GymStaff}")]
public record GetGymMembershipsQueryToMyGymQuery(GymMembershipStatus? GymMembershipStatus) 
    : IRequest<Result<List<GymMembershipWithUserProfileAndEmailDto>>>;

public class GetGymMembershipsQueryToMyGymQueryHandler : IRequestHandler<GetGymMembershipsQueryToMyGymQuery, Result<List<GymMembershipWithUserProfileAndEmailDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly IQueryService _queryService;


    public GetGymMembershipsQueryToMyGymQueryHandler(
        IApplicationDbContext context,
        IUser user,
        IQueryService queryService
    )
    {
        _context = context;
        _queryService = queryService;
        _user = user;
    }

    public async Task<Result<List<GymMembershipWithUserProfileAndEmailDto>>> Handle(GetGymMembershipsQueryToMyGymQuery query, CancellationToken cancellationToken)
    {
        var gymEmployment = await _context
            .GymEmployments
            .AsNoTracking()
            .FirstOrDefaultAsync(ge => ge.UserId != null && ge.UserId == _user.Id);

        Guard.Against.NullEntityRelatedToCurrentUser(gymEmployment, nameof(GymEmployment), _user.Id);

        var memberships = await _queryService.GetGymMembershipsWithUserProfilesAndEmailByGymIdAndMembershipStatus(gymEmployment.GymId!, query.GymMembershipStatus);

        return Result.Success(memberships);
    }
}