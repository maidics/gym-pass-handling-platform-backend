using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Security;
using FitPass.Application.NonRegisteredUsers.DTOs;
using FitPass.Domain.Constants;

namespace FitPass.Application.NonRegisteredUsers.Queries;

[Authorize(Roles = $"{Roles.GymAdministrator},{Roles.GymStaff}")]
public record GetAllMyNonRegisteredUsersQuery : IRequest<List<NonRegisteredUserDto>>;

public class GetAllMyNonRegisteredUsersQueryHandler : IRequestHandler<GetAllMyNonRegisteredUsersQuery, List<NonRegisteredUserDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly IUserProfileService _userProfileService;
    private readonly IMapper _mapper;

    public GetAllMyNonRegisteredUsersQueryHandler(IApplicationDbContext context, IUser user, IUserProfileService userProfileService, IMapper mapper)
    {
        _context = context;
        _user = user;
        _userProfileService = userProfileService;
        _mapper = mapper;
    }

    public async Task<List<NonRegisteredUserDto>> Handle(GetAllMyNonRegisteredUsersQuery request, CancellationToken cancellationToken)
    {
        var gymStaffAssignment = await _userProfileService.GetUserGymStaffAssigmentAsync(_user.Id!, cancellationToken);

        var nonRegisteredUsers = await _context
            .NonRegisteredUsers
            .Include(nru => nru.UserGymMemberships)
            .Where(nru => nru.UserGymMemberships.Any(ugm => ugm.GymId == gymStaffAssignment!.GymId))
            .ToListAsync();

        return _mapper.Map<List<NonRegisteredUserDto>>(nonRegisteredUsers);
    }
}
