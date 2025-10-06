using FitPass.Application.ApplicationUsers.DTOs;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Security;
using FitPass.Domain.Constants;

namespace FitPass.Application.Gyms.Queries;

[Authorize(Roles = $"{Roles.GymAdministrator},{Roles.GymStaff}")]
public record GetMyGymManagementUsersQuery : IRequest<List<ApplicationUserDto>>;

public class GetMyGymManagementUsersQueryHandler : IRequestHandler<GetMyGymManagementUsersQuery, List<ApplicationUserDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IUser _user;

    public GetMyGymManagementUsersQueryHandler(IApplicationDbContext context, IMapper mapper, IUser user)
    {
        _context = context;
        _mapper = mapper;
        _user = user;
    }
    public async Task<List<ApplicationUserDto>> Handle(GetMyGymManagementUsersQuery request, CancellationToken cancellationToken)
    {
        var gymStaffManagement = await _context.GymStaffAssigments
            .AsNoTracking()
            .FirstOrDefaultAsync(gsa => gsa.ApplicationUserId == _user.Id, cancellationToken);

        var users = await _context
            .ApplicationUsers
            .AsNoTracking()
            .Where(au => au.UserGymMemberships != null)
            .Include(au => au.UserGymMemberships!.Where(ugm => ugm.GymId == gymStaffManagement!.GymId))
            .ToListAsync(cancellationToken);

        return _mapper.Map<List<ApplicationUserDto>>(users);
    }
}
