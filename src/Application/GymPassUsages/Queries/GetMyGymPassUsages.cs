using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Security;
using FitPass.Application.GymPassUsages.DTOs;
using FitPass.Domain.Constants;

namespace FitPass.Application.GymPassUsages.Queries;

[Authorize(Roles = Roles.User)]
public record GetMyGymPassUsagesQuery : IRequest<List<GymPassUsageDto>>;

public class GetMyGymPassUsagesQueryHandler : IRequestHandler<GetMyGymPassUsagesQuery, List<GymPassUsageDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public GetMyGymPassUsagesQueryHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }
    
    public async Task<List<GymPassUsageDto>> Handle(GetMyGymPassUsagesQuery request, CancellationToken cancellationToken)
    {
        return await (
            from gpu in _context.GymPassUsages
            where gpu.UserId == _user.Id
            select new GymPassUsageDto(
                gpu.Id,
                null,
                null,
                gpu.GymId,
                gpu.PassType,
                gpu.TotalPassUses,
                gpu.RemainingPassUses,
                gpu.PassExpirationDate,
                gpu.PassUseResult,
                gpu.LockerNumber,
                gpu.CreatedOn,
                gpu.GymSessionEndedAt)).ToListAsync(cancellationToken);
    }
}
