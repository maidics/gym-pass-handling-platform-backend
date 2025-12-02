using FitPass.Application.Gyms.DTOs;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Security;
using FitPass.Domain.Constants;

namespace FitPass.Application.Gyms.Queries;

[Authorize(Roles = Roles.AppAdministrator)]
public record GetNewGymsThisMonthQuery : IRequest<List<GymDto>>;

public class GetNewGymsThisMonthHandler : IRequestHandler<GetNewGymsThisMonthQuery, List<GymDto>>
{
    private readonly TimeProvider _timeProvider;
    private readonly IApplicationDbContext _context;

    public GetNewGymsThisMonthHandler(
        TimeProvider timeProvider,
        IApplicationDbContext context)
    {
        _timeProvider = timeProvider;
        _context = context;
    }

    public async Task<List<GymDto>> Handle(GetNewGymsThisMonthQuery request, CancellationToken cancellationToken)
    {
        var now = _timeProvider.GetUtcNow();
        var startOfThisMonth = new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, TimeSpan.Zero);
        var startOfNextMonth = startOfThisMonth.AddMonths(1);

        var newGyms = await _context
            .Gyms
            .AsNoTracking()
            .Where(g => g.CreatedOn >= startOfThisMonth && g.CreatedOn < startOfNextMonth)
            .ToListAsync(cancellationToken);

        return newGyms.Select(g => g.MapToDto()).ToList();
    }
}
