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
            .FirstOrDefaultAsync(ge => ge.UserId == _user.Id, cancellationToken);

        Guard.Against.NullParameterRelatedToCurrentUser(gymEmployment, nameof(GymEmployment), _user.Id);

        var utcNow = _timeProvider.GetUtcNow();
        var start = utcNow.UtcDateTime.Date;
        var end = start.AddDays(1);

        return await (
            from gpu in _context.GymPassUsages join
                up in _context.UserProfiles on gpu.UserId equals up.UserId
            where gpu.GymId == gymEmployment.GymId && //no sql date conversions
                  gpu.CreatedOn >= start &&
                  gpu.CreatedOn <= end
            orderby gpu.CreatedOn descending 
            select new GymPassUsageDto(
                gpu.Id,
                up.FirstName,
                up.LastName,
                gpu.GymId,
                gpu.PassType,
                gpu.TotalPassUses,
                gpu.RemainingPassUses,
                gpu.PassExpirationDate,
                gpu.PassUseResult,
                gpu.LockerNumber,
                gpu.CreatedOn,
                gpu.GymSessionEndedAt)
            ).ToListAsync(cancellationToken);
    }
}
