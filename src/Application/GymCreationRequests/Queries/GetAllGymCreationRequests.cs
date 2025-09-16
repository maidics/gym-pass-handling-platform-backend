using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Security;
using FitPass.Application.Requests.DTOs;
using FitPass.Domain.Constants;

namespace Fitpass.Application.GymCreationRequests.Queries;

[Authorize(Roles = Roles.AppAdministrator)]
public record GetAllGymCreationRequestsQuery : IRequest<List<GymCreationRequestDto>>;

public class GetAllGymCreationRequestsQueryHandler : IRequestHandler<GetAllGymCreationRequestsQuery, List<GymCreationRequestDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetAllGymCreationRequestsQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }
    public async Task<List<GymCreationRequestDto>> Handle(GetAllGymCreationRequestsQuery request, CancellationToken cancellationToken)
    {
        var gymCreationRequests = await _context.GymCreationRequests.AsNoTracking().ToListAsync(cancellationToken);

        return _mapper.Map<List<GymCreationRequestDto>>(gymCreationRequests);
    }
}