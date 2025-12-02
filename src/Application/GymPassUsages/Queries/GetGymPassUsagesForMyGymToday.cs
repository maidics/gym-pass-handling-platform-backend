using FitPass.Application.Common.Extensions;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Security;
using FitPass.Application.GymPassUsages.DTOs;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;

namespace FitPass.Application.GymPassUsages.Queries;

[Authorize(Roles = $"{Roles.GymAdministrator},{Roles.GymStaff}")]
public record GetGymPassUsagesForMyGymTodayQuery : IRequest<List<GymPassUsageDto>>;

public class GetGymPassUsagesForMyGymTodayQueryHandler : IRequestHandler<GetGymPassUsagesForMyGymTodayQuery, List<GymPassUsageDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly TimeProvider _timeProvider;

    public GetGymPassUsagesForMyGymTodayQueryHandler(
        IApplicationDbContext context,
        IUser user,
        TimeProvider timeProvider)
    {
        _context = context;
        _user = user;
        _timeProvider = timeProvider;
    }
    public async Task<List<GymPassUsageDto>> Handle(GetGymPassUsagesForMyGymTodayQuery request, CancellationToken cancellationToken)
    {
        var gymEmployment = await _context
            .GymEmployments
            .AsNoTracking()
            .FirstOrDefaultAsync(ge => ge.UserId != null && ge.UserId == _user.Id);

        Guard.Against.NullParameterRelatedToCurrentUser(gymEmployment, nameof(GymEmployment), _user.Id);

        var gymPassUsages =  await _context
            .GymPassUsages
            .Where(gpu => gpu.CreatedOn.IsToday(_timeProvider.GetUtcNow()) && gpu.GymId == gymEmployment.GymId)
            .OrderByDescending(gpu => gpu.CreatedOn)
            .ToListAsync();

        return gymPassUsages.Select(gpu => gpu.MapToDto()).ToList();
    }
}
