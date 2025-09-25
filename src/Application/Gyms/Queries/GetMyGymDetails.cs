using Fitpass.Application.Gyms.DTOs;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Security;
using FitPass.Domain.Constants;

namespace FitPass.Application.Gyms.Queries;

[Authorize(Roles = $"{Roles.GymAdministrator},{Roles.GymStaff}")]
public record GetMyGymDetailsQuery : IRequest<GymDto>;

public class GetMyGymDetailsQueryHandler : IRequestHandler<GetMyGymDetailsQuery, GymDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly IUserProfileService _userProfileService;
    private readonly IMapper _mapper;

    public GetMyGymDetailsQueryHandler(IApplicationDbContext context, IUser user, IUserProfileService userProfileService, IMapper mapper)
    {
        _context = context;
        _user = user;
        _userProfileService = userProfileService;
        _mapper = mapper;
    }
    public async Task<GymDto> Handle(GetMyGymDetailsQuery query, CancellationToken cancellationToken)
    {
        var gymStaffAssigment = await _userProfileService.GetUserGymStaffAssigmentAsync(_user.Id!, cancellationToken);

        Guard.Against.Null(gymStaffAssigment, "Id", "Failed to find currently logged in Gym admin or Gym Staff member.");

        var gym = await _context
            .Gyms
            .AsNoTracking()
            .Include(g => g.GymPassProducts)
            .FirstOrDefaultAsync(g => g.Id == gymStaffAssigment.GymId, cancellationToken);

        Guard.Against.Null(gym, "Id", "Failed to find gym for the current Gym Admin or Gym Staff member.");

        return _mapper.Map<GymDto>(gym);
    }
}