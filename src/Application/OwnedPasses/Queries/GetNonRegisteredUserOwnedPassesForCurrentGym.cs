using FitPass.Application.Common.Interfaces;
using FitPass.Application.Extensions;
using FitPass.Application.OwnedPasses.DTOs;

namespace FitPass.Application.OwnedPasses.Queries;

public record GetNonRegisteredUserOwnedPassesForCurrentGymQuery
    (
        string NonRegisteredUserId
    ) : IRequest<List<OwnedPassDto>>;

public class GetNonRegisteredUserOwnedPassesForCurrentGymQueryValidator : AbstractValidator<GetNonRegisteredUserOwnedPassesForCurrentGymQuery>
{
    public GetNonRegisteredUserOwnedPassesForCurrentGymQueryValidator()
    {
        RuleFor(v => v.NonRegisteredUserId).NotEmptyWithMessage(nameof(GetNonRegisteredUserOwnedPassesForCurrentGymQuery.NonRegisteredUserId));
    }
}

public class GetNonRegisteredUserOwnedPassesForCurrentGymQueryHandler : IRequestHandler<GetNonRegisteredUserOwnedPassesForCurrentGymQuery, List<OwnedPassDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly IMapper _mapper;

    public GetNonRegisteredUserOwnedPassesForCurrentGymQueryHandler(IApplicationDbContext context, IUser user, IMapper mapper)
    {
        _context = context;
        _user = user;
        _mapper = mapper;
    }
    public async Task<List<OwnedPassDto>> Handle(GetNonRegisteredUserOwnedPassesForCurrentGymQuery query, CancellationToken cancellationToken)
    {
        var gymStaffAssigment = await _context.GymEmployments
            .AsNoTracking()
            .FirstOrDefaultAsync(gsa => gsa.ApplicationUserId == _user.Id, cancellationToken);

        Guard.Against.Null(gymStaffAssigment, "Gym staff assignment", "Failed to find currently logged in Gym Admin or Gym Staff member.");

        var nonRegisteredUser = await _context
            .NonRegisteredUsers
            .Include(nru => nru.UserGymMemberships.Where(ugm => ugm.GymId == gymStaffAssigment.GymId))
            .ThenInclude(ugm => ugm.OwnedPasses)
            .AsNoTracking()
            .FirstOrDefaultAsync(nru => nru.Id == query.NonRegisteredUserId, cancellationToken);

        Guard.Against.NotFound(query.NonRegisteredUserId, nonRegisteredUser, "Non registered user");

        var passes = nonRegisteredUser.UserGymMemberships.First().OwnedPasses;

        return _mapper.Map<List<OwnedPassDto>>(passes);
    }
}
