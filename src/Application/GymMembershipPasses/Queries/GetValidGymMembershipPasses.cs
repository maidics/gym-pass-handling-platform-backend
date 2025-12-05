using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Security;
using FitPass.Application.GymMembershipPasses.DTOs;
using FitPass.Domain.Constants;

namespace FitPass.Application.GymMembershipPasses.Queries;

[Authorize(Roles = Roles.User)]
public record GetValidGymMembershipPassesQuery : IRequest<List<GymMembershipPassDto>>;

public class GetValidGymMembershipPassesQueryHandler : IRequestHandler<GetValidGymMembershipPassesQuery, List<GymMembershipPassDto>>
{
    private readonly TimeProvider _timeProvider;
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

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
        var passes = await _context.GymMembershipPasses
            .Where(x => x.UserId == _user.Id && !x.IsValid(_timeProvider.GetUtcNow())) 
            .ToListAsync(cancellationToken);
            
        return passes.Select(x => x.MapToDto()).ToList();
    }
}
