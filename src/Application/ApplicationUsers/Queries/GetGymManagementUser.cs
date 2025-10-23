using FitPass.Application.ApplicationUsers.DTOs;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Security;
using FitPass.Application.Extensions;
using FitPass.Domain.Constants;

namespace FitPass.Application.ApplicationUsers.Queries;

[Authorize(Roles = $"{Roles.GymAdministrator},{Roles.GymStaff}")]
public record GetGymManagementUserQuery(string GymManagementUserId) : IRequest<ApplicationUserDto>;

public class GetGymManagementUserQueryValidator : AbstractValidator<GetGymManagementUserQuery>
{
    public GetGymManagementUserQueryValidator()
    {
        RuleFor(v => v.GymManagementUserId).NotEmptyWithMessage("Gym management user id");
    }
}

public class GetGymManagementUserQueryHandler : IRequestHandler<GetGymManagementUserQuery, ApplicationUserDto>
{
    private readonly IIdentityService _identityService;
    private readonly IMapper _mapper;
    private readonly IUser _user;
    private readonly IApplicationDbContext _context;

    public GetGymManagementUserQueryHandler(IIdentityService identityService, IMapper mapper, IUser user, IApplicationDbContext context)
    {
        _identityService = identityService;
        _mapper = mapper;
        _user = user;
        _context = context;
    }

    public async Task<ApplicationUserDto> Handle(GetGymManagementUserQuery query, CancellationToken cancellationToken)
    {
        var user = await _identityService.FindUserByIdAsync(query.GymManagementUserId, cancellationToken);

        Guard.Against.NotFound(query.GymManagementUserId, user, "Gym management user");

        if (user.GymStaffAssignment == null)
        {
            throw new UnauthorizedAccessException();
        }

        var gymStaffAssignment = await _context.GymStaffAssigments.AsNoTracking().FirstOrDefaultAsync(gsa => gsa.ApplicationUserId == _user!.Id, cancellationToken);

        if (user.GymStaffAssignment.GymId != gymStaffAssignment!.GymId)
        {
            throw new UnauthorizedAccessException();
        }

        return _mapper.Map<ApplicationUserDto>(user);
    }
}
