using FitPass.Application.Common.Exceptions;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Logging;
using FitPass.Application.Common.Security;
using FitPass.Application.GymEmployments.DTOs;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using FitPass.Domain.Strings;
using Microsoft.Extensions.Logging;

namespace Fitpass.Application.GymEmployments.Queries;

[Authorize(Roles = $"{Roles.GymAdministrator},{Roles.GymStaff}")]
public record GetMyGymEmploymentsQuery : IRequest<List<GymEmploymentDto>>;

public class GetMyGymEmploymentsQueryHandler : IRequestHandler<GetMyGymEmploymentsQuery, List<GymEmploymentDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly ILogger<GetMyGymEmploymentsQuery> _logger;
    private readonly IQueryService _queryService;

    public GetMyGymEmploymentsQueryHandler(IApplicationDbContext context, IUser user, ILogger<GetMyGymEmploymentsQuery> logger, IQueryService queryService)
    {
        _context = context;
        _user = user;
        _logger = logger;
        _queryService = queryService;
    }
    public async Task<List<GymEmploymentDto>> Handle(GetMyGymEmploymentsQuery query, CancellationToken cancellationToken)
    {
        var gymEmployment = await _context.GymEmployments
            .AsNoTracking()
            .FirstOrDefaultAsync(ge => ge.ApplicationUserId != null && ge.ApplicationUserId == _user.Id, cancellationToken);

        if (gymEmployment == null)
        {
            LogCriticalMessages.AuthenticatedUserRelatedEntityNotFound(_logger, _user.Roles, _user.Id, nameof(GymEmployment));
            throw new Exception(ErrorMessages.AuthenticatedUserRelatedEntityNotFound(nameof(GymEmployment)));
        }

        if (gymEmployment.GymId == null)
        {
            throw new ForbiddenAccessException();
        }

        return await _queryService.GetGymEmploymentsWithUserProfileAndEmailByGymId(gymEmployment.GymId);
    }
}
