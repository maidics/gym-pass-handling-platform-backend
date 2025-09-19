using Fitpass.Application.Gyms.DTOs;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Security;
using FitPass.Application.Extensions;
using FitPass.Domain.Constants;

namespace Fitpass.Application.Gyms.Queries;

[Authorize(Roles = $"{Roles.AppAdministrator},{Roles.GymAdministrator},{Roles.GymStaff}")]
public record GetGymDetailsQuery(string GymId) : IRequest<GymDto?>;

public class GetGymDetailsQueryValidator : AbstractValidator<GetGymDetailsQuery>
{
    public GetGymDetailsQueryValidator()
    {
        RuleFor(v => v.GymId).NotEmptyWithMessage("Gym id");
    }
}

public class GetGymDetailsQueryHandler : IRequestHandler<GetGymDetailsQuery, GymDto?>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetGymDetailsQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<GymDto?> Handle(GetGymDetailsQuery query, CancellationToken cancellationToken)
    {
        var gym = await _context
            .Gyms
            .AsNoTracking()
            .Include(g => g.GymPassProducts)
            .Include(g => g.UserGymMemberships)
            .FirstOrDefaultAsync(g => g.Id == query.GymId);

        return gym == null ? null : _mapper.Map<GymDto>(gym);
    }
}