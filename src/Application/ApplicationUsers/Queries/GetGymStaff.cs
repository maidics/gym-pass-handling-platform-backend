using FitPass.Application.ApplicationUsers.DTOs;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Security;
using FitPass.Application.Extensions;
using FitPass.Domain.Constants;

namespace Fitpass.Application.ApplicationUsers.Queries;

[Authorize(Roles = Roles.AppAdministrator)]
public record GetGymStaffQuery(string GymId) : IRequest<List<ApplicationUserDto>>;

public class GetGymStaffQueryValidator : AbstractValidator<GetGymStaffQuery>
{
    public GetGymStaffQueryValidator()
    {
        RuleFor(v => v.GymId).NotEmptyWithMessage("Gym id");
    }
}

public class GetGymStaffQueryHandler : IRequestHandler<GetGymStaffQuery, List<ApplicationUserDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetGymStaffQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<ApplicationUserDto>> Handle(GetGymStaffQuery query, CancellationToken cancellationToken)
    {
        var gym = await _context.Gyms.AsNoTracking().FirstOrDefaultAsync(g => g.Id == query.GymId, cancellationToken);

        Guard.Against.NotFound(query.GymId, gym, "GymId");

        var users = await _context
            .ApplicationUsers
            .Where(au => au.GymStaffAssignment != null && au.GymStaffAssignment.GymId == query.GymId)
            .Include(au => au.GymStaffAssignment)
            .ToListAsync(cancellationToken);

        return _mapper.Map<List<ApplicationUserDto>>(users);
    }
}
