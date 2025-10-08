using FitPass.Application.ApplicationUsers.DTOs;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Security;
using FitPass.Domain.Constants;

namespace Fitpass.Application.ApplicationUsers.Queries;

[Authorize(Roles = $"{Roles.GymAdministrator},{Roles.GymStaff}")]
public record GetAllMyGymStaffQuery : IRequest<List<ApplicationUserDto>>;

public class GetAllMyGymStaffQueryHandler : IRequestHandler<GetAllMyGymStaffQuery, List<ApplicationUserDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly IMapper _mapper;

    public GetAllMyGymStaffQueryHandler(IApplicationDbContext context, IUser user, IMapper mapper)
    {
        _context = context;
        _user = user;
        _mapper = mapper;
    }
    public async Task<List<ApplicationUserDto>> Handle(GetAllMyGymStaffQuery query, CancellationToken cancellationToken)
    {
        var gymStaffAssigment = await _context.GymStaffAssigments
            .AsNoTracking()
            .FirstOrDefaultAsync(gsa => gsa.ApplicationUserId == _user.Id, cancellationToken);

        var gymStaffMembers = await _context
            .ApplicationUsers
            .Where(au => au.GymStaffAssignment != null && au.GymStaffAssignment.GymId == gymStaffAssigment!.GymId)
            .Include(au => au.GymStaffAssignment)
            .ToListAsync(cancellationToken);

        return _mapper.Map<List<ApplicationUserDto>>(gymStaffMembers);
    }
}
