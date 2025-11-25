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
    private readonly TimeProvider _timeProvider;
    private readonly IApplicationDbContext _context;
    private readonly IUser _user; //currently logged in user

    public GetGymMembershipPassesForGymQueryHandler(
        TimeProvider timeProvider,
        IApplicationDbContext context, 
        IUser user)
    {
        _timeProvider = timeProvider;
        _context = context;
        _user = user;
    }
    public async Task<List<GymMembershipPassDto>> Handle(GetGymMembershipPassesForGymQuery query, CancellationToken cancellationToken)
    {
        var utcNow = _timeProvider.GetUtcNow();

        var gymMembership = await _context.GymMemberships
            .AsNoTracking()
            .Include(gm => 
                gm.Passes.Where(p => 
                    (p.RemainingUses != null && p.RemainingUses > 0) || 
                    (p.ExpirationDate != null && p.IsExpired(utcNow)))) //p.ExpirationDate.Value.UtcDateTime.Date < utcNow.UtcDateTime.Date
            .FirstOrDefaultAsync(gm => gm.UserId == _user.Id && gm.GymId == query.GymId);

        Guard.Against.NotFound(query.GymId, gymMembership);

        return gymMembership.Passes.Select(p => p.MapToDto()).ToList();
    }
}
