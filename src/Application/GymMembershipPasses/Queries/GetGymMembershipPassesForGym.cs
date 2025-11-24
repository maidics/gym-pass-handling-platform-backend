using FitPass.Application.Common.Extensions;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Security;
using FitPass.Application.GymMembershipPasses.DTOs;

namespace FitPass.Application.GymMembershipPasses.Queries;

[Authorize]
public record GetGymMembershipPassesForGymQuery
    (
        string GymId //gym id from scanned qr code or gym's profile
    ) : IRequest<List<GymMembershipPassDto>>;

public class GetGymMembershipPassesForGymQueryValidator : AbstractValidator<GetGymMembershipPassesForGymQuery>
{
    public GetGymMembershipPassesForGymQueryValidator()
    {
        RuleFor(v => v.GymId).NotEmptyWithMessage(nameof(GetGymMembershipPassesForGymQuery.GymId));
    }
}

public class GetGymMembershipPassesForGymQueryHandler : IRequestHandler<GetGymMembershipPassesForGymQuery, List<GymMembershipPassDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user; //currently logged in user

    public GetGymMembershipPassesForGymQueryHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }
    public async Task<List<GymMembershipPassDto>> Handle(GetGymMembershipPassesForGymQuery query, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var today = new DateOnly(now.Year, now.Month, now.Day);

        var gymMembership = await _context.GymMemberships
            .AsNoTracking()
            .Include(gm => gm.Passes.Where(p => (p.RemainingUses != null && p.RemainingUses > 0) || (p.ExpirationDate != null && p.ExpirationDate < today)))
            .FirstOrDefaultAsync(gm => gm.UserId == _user.Id && gm.GymId == query.GymId);

        Guard.Against.Null(gymMembership, _user.Id, "User is not a member of this gym.");

        return gymMembership.Passes.Select(p => p.MapToDto()).ToList();
    }
}
