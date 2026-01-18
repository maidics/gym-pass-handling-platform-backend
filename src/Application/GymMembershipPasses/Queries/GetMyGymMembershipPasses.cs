using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Security;
using FitPass.Application.GymMembershipPasses.DTOs;
using FitPass.Domain.Constants;

namespace FitPass.Application.GymMembershipPasses.Queries;

[Authorize(Roles = Roles.User)]
public record GetMyGymMembershipPassesQuery : IRequest<List<GymMembershipPassDto>>;

public class GetMyGymMembershipPassesQueryHandler : IRequestHandler<GetMyGymMembershipPassesQuery, List<GymMembershipPassDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public GetMyGymMembershipPassesQueryHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }
    
    public async Task<List<GymMembershipPassDto>> Handle(GetMyGymMembershipPassesQuery request, CancellationToken cancellationToken)
    {
        var passes = await _context.GymMembershipPasses
            .AsNoTracking()
            .Where(x => x.UserId == _user.Id)
            .ToListAsync(cancellationToken);

        return passes.Select(x => x.MapToDto()).ToList();
    }
}
