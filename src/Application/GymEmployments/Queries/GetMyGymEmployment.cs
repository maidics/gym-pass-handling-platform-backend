using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Logging;
using FitPass.Application.Common.Security;
using FitPass.Application.GymEmployments.DTOs;
using FitPass.Domain.Constants;
using FitPass.Domain.Strings;
using Microsoft.Extensions.Logging;

namespace FitPass.Application.GymEmployments.Queries;

[Authorize(Roles = $"{Roles.GymAdministrator},{Roles.GymStaff}")]
public record GetMyGymEmploymentQuery : IRequest<GymEmploymentDto>;

public class GetMyGymEmploymentQueryHandler : IRequestHandler<GetMyGymEmploymentQuery, GymEmploymentDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly ILogger<GetMyGymEmploymentQueryHandler> _logger;
    private readonly IQueryService _queryService;

    public GetMyGymEmploymentQueryHandler(
        IApplicationDbContext context,
        IUser user,
        ILogger<GetMyGymEmploymentQueryHandler> logger,
        IQueryService queryService)
    {
        _context = context;
        _user = user;
        _logger = logger;
        _queryService = queryService;
    }

    public async Task<GymEmploymentDto> Handle(GetMyGymEmploymentQuery request, CancellationToken cancellationToken)
    {
        var gymEmployment = await _context
            .GymEmployments
            .AsNoTracking()
            .FirstOrDefaultAsync(ge => ge.UserId != null && ge.UserId == _user.Id);

        if (gymEmployment == null)
        {
            LogCriticalMessages.AuthenticatedUserRelatedEntityNotFound(_logger, _user.Roles, _user.Id, nameof(gymEmployment));
            throw new SystemException(ErrorMessages.AuthenticatedUserRelatedEntityNotFound(nameof(gymEmployment)));
        }

        var gymEmploymentDto = await _queryService.GetGymEmploymentWithUserProfileAndEmailByApplicationUserId(_user.Id!);

        return gymEmploymentDto!;
    }
}