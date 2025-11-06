using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Logging;
using FitPass.Application.Common.Security;
using FitPass.Application.GymMemberships.DTOs;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using FitPass.Domain.Enums;
using FitPass.Domain.Strings;
using Microsoft.Extensions.Logging;

namespace FitPass.Application.GymMemberships.Queries;

[Authorize(Roles = $"{Roles.GymAdministrator},{Roles.GymStaff}")]
public record GetGymMembershipsQueryToMyGymQuery(GymMembershipStatus? GymMembershipStatus) 
    : IRequest<List<GymMembershipWithUserProfileAndEmailDto>>;

public class GetGymMembershipsQueryToMyGymQueryHandler : IRequestHandler<GetGymMembershipsQueryToMyGymQuery, List<GymMembershipWithUserProfileAndEmailDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly IQueryService _queryService;
    private readonly ILogger<GetGymMembershipsQueryToMyGymQueryHandler> _logger;


    public GetGymMembershipsQueryToMyGymQueryHandler(
        IApplicationDbContext context,
        IUser user,
        ILogger<GetGymMembershipsQueryToMyGymQueryHandler> logger,
        IQueryService queryService
    )
    {
        _context = context;
        _queryService = queryService;
        _user = user;
        _logger = logger;
    }

    public async Task<List<GymMembershipWithUserProfileAndEmailDto>> Handle(GetGymMembershipsQueryToMyGymQuery query, CancellationToken cancellationToken)
    {
        var gymEmployment = await _context
            .GymEmployments
            .AsNoTracking()
            .FirstOrDefaultAsync(ge => ge.ApplicationUserId != null && ge.ApplicationUserId == _user.Id);

        if (gymEmployment is null)
        {
            LogCriticalMessages.AuthenticatedUserRelatedEntityNotFound(
                _logger,
                _user.Roles,
                _user.Id,
                nameof(GymEmployment));

            throw new SystemException(ErrorMessages.AuthenticatedUserRelatedEntityNotFound(nameof(GymEmployment)));
        }

        return await _queryService.GetGymMembershipsWithUserProfilesAndEmailByGymIdAndMembershipStatus(gymEmployment.GymId!, query.GymMembershipStatus);
    }
}