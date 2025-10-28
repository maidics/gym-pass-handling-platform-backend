using FitPass.Application.ApplicationUsers.DTOs;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Logging;
using FitPass.Application.Common.Security;
using FitPass.Domain.Constants;
using Microsoft.Extensions.Logging;

namespace Fitpass.Application.ApplicationUsers.Queries;

[Authorize(Roles = $"{Roles.GymAdministrator},{Roles.GymStaff}")]
public record GetAllMyGymStaffQuery : IRequest<List<ApplicationUserDto>>; //return this here: (UserProfile, GymEmployment)[] or - array of tuples

public class GetAllMyGymStaffQueryHandler : IRequestHandler<GetAllMyGymStaffQuery, List<ApplicationUserDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly IMapper _mapper;
    private readonly ILogger<GetAllMyGymStaffQuery> _logger;

    public GetAllMyGymStaffQueryHandler(IApplicationDbContext context, IUser user, IMapper mapper, ILogger<GetAllMyGymStaffQuery> logger)
    {
        _context = context;
        _user = user;
        _mapper = mapper;
        _logger = logger;
    }
    public async Task<List<ApplicationUserDto>> Handle(GetAllMyGymStaffQuery query, CancellationToken cancellationToken)
    {
        var currentGymEmployment = await _context.GymEmployments
            .AsNoTracking()
            .FirstOrDefaultAsync(gsa => gsa.ApplicationUserId == _user.Id, cancellationToken);

        if (currentGymEmployment == null)
        {
            LogCriticalMessages.AuthenticatedGymEmployeeGymEmploymentNotFound(_logger, _user.Roles, _user.Id, null);
        }

        var gymStaffEmployments = await _context
            .GymEmployments
            .Include(ge => ge.ApplicationUser)

        return _mapper.Map<List<ApplicationUserDto>>(gymStaffEmployments);
    }
}
