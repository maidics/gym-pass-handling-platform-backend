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

    public GetMyGymQueryHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }
    public async Task<GymDto> Handle(GetMyGymQuery query, CancellationToken cancellationToken)
    {
        var gymEmployment = await _context.GymEmployments
            .AsNoTracking()
            .FirstOrDefaultAsync(gsa => gsa.UserId == _user.Id, cancellationToken);

        Guard.Against.NullParameterRelatedToCurrentUser(gymEmployment, nameof(GymEmployment), _user.Id);

        var gymQuery = _context
            .Gyms
            .AsNoTracking()
            .Where(x => x.Id == gymEmployment.GymId);

        if (gymEmployment.Role == Roles.GymAdministrator)
        {
            gymQuery.Include((x => x.PaymentProfile));
        }

        var gym = await gymQuery.FirstOrDefaultAsync(cancellationToken);

        Guard.Against.NullParameterRelatedToCurrentUser(gym, "gym employee managed gym", _user.Id);

        return gym.MapToDto();
    }
}
