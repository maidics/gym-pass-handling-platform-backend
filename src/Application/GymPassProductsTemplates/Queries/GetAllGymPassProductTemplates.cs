using Fitpass.Application.GymPassProductsTemplates.DTOs;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Security;
using FitPass.Domain.Constants;

namespace Fitpass.Application.GymPassProductsTemplates.Queries;

[Authorize(Roles = $"{Roles.AppAdministrator},{Roles.GymAdministrator}")]
public record GetAllGymPassProductTemplatesQuery : IRequest<List<GymPassProductTemplateDto>>;

public class GetAllGymPassProductTemplatesQueryHandler : IRequestHandler<GetAllGymPassProductTemplatesQuery, List<GymPassProductTemplateDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    public GetAllGymPassProductTemplatesQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<GymPassProductTemplateDto>> Handle(GetAllGymPassProductTemplatesQuery request, CancellationToken cancellationToken)
    {
        var templates = await _context
            .GymPassProductTemplates
            .AsNoTracking()
            .ToListAsync();

        return _mapper.Map<List<GymPassProductTemplateDto>>(templates);
    }
}