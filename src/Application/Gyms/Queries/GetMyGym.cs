using FitPass.Application.Gyms.DTOs;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Security;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using Microsoft.Extensions.Logging;
using FitPass.Application.Common.Extensions;

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
            .FirstOrDefaultAsync(gsa => gsa.UserId == _user.Id, cancellationToken);

        Guard.Against.NullParameterRelatedToCurrentUser(gymEmployment, nameof(GymEmployment), _user.Id);

        var gym = await _context
            .Gyms
            .AsNoTracking()
            .FirstOrDefaultAsync(g => g.Id == gymEmployment.GymId, cancellationToken);

        Guard.Against.NullParameterRelatedToCurrentUser(gym, "gym employee managed gym", _user.Id);

        return gym.MapToDto();
    }
}
