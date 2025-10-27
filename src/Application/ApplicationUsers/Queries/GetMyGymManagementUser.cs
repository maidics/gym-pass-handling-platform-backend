using FitPass.Application.ApplicationUsers.DTOs;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Security;
using FitPass.Application.Extensions;
using FitPass.Domain.Constants;

namespace FitPass.Application.ApplicationUsers.Queries;

[Authorize(Roles = $"{Roles.GymAdministrator},{Roles.GymStaff}")]
public record GetMyGymManagementUserQuery(string GymManagementUserId) : IRequest<ApplicationUserDto>;

public class GetMyGymManagementUserQueryValidator : AbstractValidator<GetMyGymManagementUserQuery>
{
    public GetMyGymManagementUserQueryValidator()
    {
        RuleFor(v => v.GymManagementUserId).NotEmptyWithMessage(nameof(GetMyGymManagementUserQuery.GymManagementUserId);
    }
}

public class GetMyGymManagementUserQueryHandler : IRequestHandler<GetMyGymManagementUserQuery, ApplicationUserDto>
{
    private readonly IIdentityService _identityService;
    private readonly IMapper _mapper;
    private readonly IUser _user;
    private readonly IApplicationDbContext _context;

    public GetMyGymManagementUserQueryHandler(IIdentityService identityService, IMapper mapper, IUser user, IApplicationDbContext context)
    {
        _identityService = identityService;
        _mapper = mapper;
        _user = user;
        _context = context;
    }

    public async Task<ApplicationUserDto> Handle(GetMyGymManagementUserQuery query, CancellationToken cancellationToken)
    {
        var user = await _identityService.FindUserByIdAsync(query.GymManagementUserId, cancellationToken);

        Guard.Against.NotFound(query.GymManagementUserId, user, "Gym management user");

        if (user.GymStaffAssignment == null)
        {
            throw new UnauthorizedAccessException();
        }

        var gymStaffAssignment = await _context.GymEmployments.AsNoTracking().FirstOrDefaultAsync(gsa => gsa.ApplicationUserId == _user!.Id, cancellationToken);

        if (user.GymStaffAssignment.GymId != gymStaffAssignment!.GymId)
        {
            throw new UnauthorizedAccessException();
        }

        return _mapper.Map<ApplicationUserDto>(user);
    }
}
