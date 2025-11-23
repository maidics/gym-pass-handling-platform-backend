using FitPass.Application.Gyms.DTOs;
using FitPass.Application.Common.Interfaces;

namespace FitPass.Application.Gyms.Queries;

public record GetAllGymsQuery : IRequest<List<GymDto>>;

public class GetAllGymsQueryHandler : IRequestHandler<GetAllGymsQuery, List<GymDto>>
{
    private readonly IApplicationDbContext _context;
    public GetAllGymsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<GymDto>> Handle(GetAllGymsQuery request, CancellationToken cancellationToken)
    {
        var gyms = await _context.Gyms.AsNoTracking().ToListAsync(cancellationToken);

        return gyms.Select(g => g.MapToDto()).ToList();
    }
}
