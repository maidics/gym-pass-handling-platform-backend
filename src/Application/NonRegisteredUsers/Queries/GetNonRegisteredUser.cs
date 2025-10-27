using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Security;
using FitPass.Application.Extensions;
using FitPass.Application.NonRegisteredUsers.DTOs;
using FitPass.Domain.Constants;

namespace Fitpass.Application.NonRegisteredUsers.Queries;

[Authorize(Roles = $"{Roles.GymAdministrator},{Roles.GymStaff}")]
public record GetNonRegisteredUserQuery(string NonRegisteredUserId) : IRequest<NonRegisteredUserDto>;

public class GetNonRegisteredUserQueryValidator : AbstractValidator<GetNonRegisteredUserQuery>
{
    public GetNonRegisteredUserQueryValidator()
    {
        RuleFor(v => v.NonRegisteredUserId).NotEmptyWithMessage(nameof(GetNonRegisteredUserQuery.NonRegisteredUserId));
    }
}

public class GetNonRegisteredUserQueryHandler : IRequestHandler<GetNonRegisteredUserQuery, NonRegisteredUserDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly IMapper _mapper;

    public GetNonRegisteredUserQueryHandler(IApplicationDbContext context, IUser user, IMapper mapper)
    {
        _context = context;
        _user = user;
        _mapper = mapper;
    }
    public async Task<NonRegisteredUserDto> Handle(GetNonRegisteredUserQuery query, CancellationToken cancellationToken)
    {
        var gymStaffAssigment = await _context.GymEmployments
            .AsNoTracking()
            .FirstOrDefaultAsync(gsa => gsa.ApplicationUserId == _user.Id, cancellationToken);

        Guard.Against.Null(gymStaffAssigment, "Id", "Failed to find currently authenticated Gym Admin or Gym Staff member.");

        var nonRegisteredUser = await _context
            .NonRegisteredUsers
            .AsNoTracking()
            .Include(nru => nru.UserGymMemberships.Where(ugm => ugm.GymId == gymStaffAssigment.GymId))
            .FirstOrDefaultAsync(nru => nru.Id == query.NonRegisteredUserId, cancellationToken);

        Guard.Against.NotFound(query.NonRegisteredUserId, nonRegisteredUser, "Id");

        return _mapper.Map<NonRegisteredUserDto>(nonRegisteredUser);
    }
}
