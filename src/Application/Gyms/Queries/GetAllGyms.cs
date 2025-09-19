using Fitpass.Application.Gyms.DTOs;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Security;
using FitPass.Domain.Constants;

namespace Fitpass.Application.Gyms.Queries;

[Authorize(Roles = Roles.AppAdministrator)]
public record GetAllGymsQuery : IRequest<List<GymDto>>;

public class GetAllGymsQueryHandler : IRequestHandler<GetAllGymsQuery, List<GymDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    public GetAllGymsQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<GymDto>> Handle(GetAllGymsQuery request, CancellationToken cancellationToken)
    {
        var gyms = await _context.Gyms.AsNoTracking().ToListAsync(cancellationToken);

        return _mapper.Map<List<GymDto>>(gyms);
    }
}