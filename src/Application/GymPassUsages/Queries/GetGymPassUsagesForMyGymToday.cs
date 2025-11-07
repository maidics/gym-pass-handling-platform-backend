using FitPass.Application.Common.Extensions;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Logging;
using FitPass.Application.Common.Security;
using FitPass.Application.GymMembershipPassUsages.DTOs;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using FitPass.Domain.Strings;
using Microsoft.Extensions.Logging;

namespace FitPass.Application.GymMembershipPassUsages.Queries;

[Authorize(Roles = $"{Roles.GymAdministrator},{Roles.GymStaff}")]
public record GetGymPassUsagesForMyGymTodayQuery : IRequest<List<GymPassUsageDto>>;

public class GetGymPassUsagesForMyGymTodayQueryHandler : IRequestHandler<GetGymPassUsagesForMyGymTodayQuery, List<GymPassUsageDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly ILogger<GetGymPassUsagesForMyGymTodayQueryHandler> _logger;
    private readonly IMapper _mapper;

    public GetGymPassUsagesForMyGymTodayQueryHandler(
        IApplicationDbContext context,
        IUser user,
        ILogger<GetGymPassUsagesForMyGymTodayQueryHandler> logger,
        IMapper mapper)
    {
        _context = context;
        _user = user;
        _logger = logger;
        _mapper = mapper;
    }
    public async Task<List<GymPassUsageDto>> Handle(GetGymPassUsagesForMyGymTodayQuery request, CancellationToken cancellationToken)
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

        return await _context
            .GymPassUsages
            .Include(gpu => gpu.Pass)
            .ThenInclude(p => p.GymMembership)
            .Where(gpu => gpu.CreatedOn.IsToday() && gpu.Pass.GymMembership.GymId == gymEmployment.GymId)
            .OrderByDescending(gpu => gpu.CreatedOn)
            .ProjectTo<GymPassUsageDto>(_mapper.ConfigurationProvider)
            .ToListAsync();
    }
}
