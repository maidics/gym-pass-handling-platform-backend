using FitPass.Application.Common.Extensions;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Security;
using FitPass.Application.GymMembershipPasses.DTOs;
using FitPass.Application.GymPassProducts.DTOs;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;

namespace FitPass.Application.GymPassProducts.Queries;

[Authorize(Roles = $"{Roles.GymAdministrator},{Roles.GymStaff}")]
public record GetMyGymPassProductsQuery : IRequest<List<GymPassProductDto>>;

public class GetMyGymPassProductsQueryHandler : IRequestHandler<GetMyGymPassProductsQuery, List<GymPassProductDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public GetMyGymPassProductsQueryHandler(
        IApplicationDbContext context,
        IUser user)
    {
        _context = context;
        _user = user;
    }
    
    public async Task<List<GymPassProductDto>> Handle(GetMyGymPassProductsQuery request, CancellationToken cancellationToken)
    {
        var gymId = await _context.GymEmployments
            .AsNoTracking()
            .Where(x => x.UserId == _user.Id)
            .Include(x => x.Gym)
            .Select(x => x.GymId)
            .FirstOrDefaultAsync(cancellationToken);

        Guard.Against.NullParameterRelatedToCurrentUser(gymId, $"{nameof(GymEmployment)}.GymId", _user.Id);

        return await _context.GymPassProducts
            .AsNoTracking()
            .Where(x => x.GymId == gymId)
            .Select(x => x.MapToDto())
            .ToListAsync(cancellationToken);
    }
}
