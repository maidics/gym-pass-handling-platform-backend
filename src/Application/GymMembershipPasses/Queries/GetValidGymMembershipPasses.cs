using FitPass.Application.Common.Extensions;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Models;
using FitPass.Application.Common.Security;
using FitPass.Application.GymMembershipPasses.DTOs;
using FitPass.Domain.Constants;

namespace FitPass.Application.GymMembershipPasses.Queries;

[Authorize(Roles = Roles.User)]
public record GetValidGymMembershipPassesQuery
    (
        string GymId //gym id from scanned qr code or gym's profile
    ) : IRequest<List<GymMembershipPassDto>>;

public class GetValidGymMembershipPassesQueryValidator : AbstractValidator<GetValidGymMembershipPassesQuery>
{
    public GetValidGymMembershipPassesQueryValidator()
    {
        RuleFor(v => v.GymId).NotEmptyWithMessage(nameof(GetValidGymMembershipPassesQuery.GymId));
    }
}

public class GetValidGymMembershipPassesQueryHandler : IRequestHandler<GetValidGymMembershipPassesQuery, List<GymMembershipPassDto>>
{
    private readonly TimeProvider _timeProvider;
    private readonly IApplicationDbContext _context;
    private readonly IUser _user; //currently logged in user

    public GetValidGymMembershipPassesQueryHandler(
        TimeProvider timeProvider,
        IApplicationDbContext context, 
        IUser user)
    {
        _timeProvider = timeProvider;
        _context = context;
        _user = user;
    }
    public async Task<List<GymMembershipPassDto>> Handle(GetValidGymMembershipPassesQuery query, CancellationToken cancellationToken)
    {
        var utcNow = _timeProvider.GetUtcNow();

        var passes = await _context.GymMembershipPasses
            .Where(x => x.UserId == _user.Id && !x.IsValid(utcNow)) //p.ExpirationDate.Value.Date < utcNow.Date
            .ToListAsync(cancellationToken);
            
        return passes.Select(x => x.MapToDto()).ToList();
    }
}
