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
    private readonly IUserProfileService _userProfileService;
    private readonly IMapper _mapper;

    public GetAllMyGymStaffQueryHandler(IApplicationDbContext context, IUser user, IUserProfileService userProfileService, IMapper mapper)
    {
        _context = context;
        _user = user;
        _userProfileService = userProfileService;
        _mapper = mapper;
    }
    public async Task<List<ApplicationUserDto>> Handle(GetAllMyGymStaffQuery query, CancellationToken cancellationToken)
    {
        var gymStaffAssigment = await _userProfileService.GetUserGymStaffAssigmentAsync(_user.Id!, cancellationToken);

        Guard.Against.Null(gymStaffAssigment, "Id", "Failed to find currently logged in Gym Admin or Gym Staff member.");

        var gymStaffMembers = await _context
            .ApplicationUsers
            .Include(au => au.GymStaffAssigment)
            .Where(au => au.GymStaffAssigment != null && au.GymStaffAssigment.GymId == gymStaffAssigment.GymId)
            .ToListAsync(cancellationToken);

        return _mapper.Map<List<ApplicationUserDto>>(gymStaffMembers);
    }
}