using FitPass.Application.Common.Extensions;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Logging;
using FitPass.Application.Common.Security;
using FitPass.Application.GymPassUsages.DTOs;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using FitPass.Domain.Strings;
using Microsoft.Extensions.Logging;

namespace FitPass.Application.GymPassUsages.Queries;

[Authorize(Roles = $"{Roles.GymAdministrator},{Roles.GymStaff}")]
public record GetGymPassUsagesForMyGymTodayQuery : IRequest<List<GymPassUsageDto>>;

public class GetGymPassUsagesForMyGymTodayQueryHandler : IRequestHandler<GetGymPassUsagesForMyGymTodayQuery, List<GymPassUsageDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly ILogger<GetGymPassUsagesForMyGymTodayQueryHandler> _logger;
    private readonly TimeProvider _timeProvider;

    public GetGymPassUsagesForMyGymTodayQueryHandler(
        IApplicationDbContext context,
        IUser user,
        ILogger<GetGymPassUsagesForMyGymTodayQueryHandler> logger,
        TimeProvider timeProvider)
    {
        _context = context;
        _user = user;
        _logger = logger;
        _timeProvider = timeProvider;
    }
    public async Task<List<GymPassUsageDto>> Handle(GetGymPassUsagesForMyGymTodayQuery request, CancellationToken cancellationToken)
    {
        var gymEmployment = await _context
            .GymEmployments
            .AsNoTracking()
            .FirstOrDefaultAsync(ge => ge.UserId != null && ge.UserId == _user.Id);

        if (gymEmployment is null)
        {
            LogCriticalMessages.AuthenticatedUserRelatedEntityNotFound(
                _logger,
                _user.Roles,
                _user.Id,
                nameof(GymEmployment));

            throw new SystemException(ErrorMessages.AuthenticatedUserRelatedEntityNotFound(nameof(GymEmployment)));
        }

        var gymPassUsages =  await _context
            .GymPassUsages
            .Where(gpu => gpu.CreatedOn.IsToday(_timeProvider.GetUtcNow()) && gpu.GymId == gymEmployment.GymId)
            .OrderByDescending(gpu => gpu.CreatedOn)
            .ToListAsync();

        return gymPassUsages.Select(gpu => gpu.MapToDto()).ToList();
    }
}
