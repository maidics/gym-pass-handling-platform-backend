using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Security;
using FitPass.Application.Extensions;
using FitPass.Application.OwnedPasses.DTOs;

namespace Fitpass.Application.OwnedPasses.Queries;

[Authorize]
public record GetUserOwnedPassesForGymQuery
    (
        string UserGymMembershipId
    ) : IRequest<List<OwnedPassDto>>;

public class GetUserOwnedPassesForGymQueryValidator : AbstractValidator<GetUserOwnedPassesForGymQuery>
{
    public GetUserOwnedPassesForGymQueryValidator()
    {
        RuleFor(v => v.UserGymMembershipId).NotEmptyWithMessage("User gym membership id");
    }
}

public class GetUserOwnedPassesForGymQueryHandler : IRequestHandler<GetUserOwnedPassesForGymQuery, List<OwnedPassDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly IMapper _mapper;

    public GetUserOwnedPassesForGymQueryHandler(IApplicationDbContext context, IUser user, IMapper mapper)
    {
        _context = context;
        _user = user;
        _mapper = mapper;
    }
    public async Task<List<OwnedPassDto>> Handle(GetUserOwnedPassesForGymQuery query, CancellationToken cancellationToken)
    {
        var userGymMembership = await _context.UserGymMemberships
            .AsNoTracking()
            .Include(ugm => ugm.OwnedPasses)
            .FirstOrDefaultAsync(ugm => ugm.Id == query.UserGymMembershipId, cancellationToken);

        Guard.Against.Null(userGymMembership, query.UserGymMembershipId, "User is not a member of this gym.");

        if (userGymMembership.ApplicationUserId != _user.Id!)
        {
            throw new UnauthorizedAccessException();
        }

        return _mapper.Map<List<OwnedPassDto>>(userGymMembership.OwnedPasses);
    }
}
