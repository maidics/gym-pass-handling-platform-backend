using FitPass.Application.Gyms.DTOs;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Logging;
using FitPass.Application.Common.Security;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using FitPass.Domain.Strings;
using Microsoft.Extensions.Logging;

namespace FitPass.Application.Gyms.Queries;

[Authorize(Roles = $"{Roles.GymAdministrator},{Roles.GymStaff}")]
public record GetMyGymQuery : IRequest<GymDto>;

public class GetMyGymQueryHandler : IRequestHandler<GetMyGymQuery, GymDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly ILogger<GetMyGymQueryHandler> _logger;

    public GetMyGymQueryHandler(IApplicationDbContext context, IUser user, ILogger<GetMyGymQueryHandler> logger)
    {
        _context = context;
        _user = user;
        _logger = logger;
    }
    public async Task<GymDto> Handle(GetMyGymQuery query, CancellationToken cancellationToken)
    {
        var gymEmployment = await _context.GymEmployments
            .AsNoTracking()
            .FirstOrDefaultAsync(gsa => gsa.ApplicationUserId == _user.Id, cancellationToken);

        if (gymEmployment == null)
        {
            LogCriticalMessages.AuthenticatedUserRelatedEntityNotFound(_logger, _user.Roles, _user.Id, nameof(GymEmployment));

            throw new SystemException(ErrorMessages.AuthenticatedUserRelatedEntityNotFound(nameof(GymEmployment)));
        }

        var gym = await _context
            .Gyms
            .AsNoTracking()
            .FirstOrDefaultAsync(g => g.Id == gymEmployment.GymId, cancellationToken);

        Guard.Against.Null(gym, "Id", "No gym found for authenticated gym employee.");

        return gym.MapToDto();
    }
}
