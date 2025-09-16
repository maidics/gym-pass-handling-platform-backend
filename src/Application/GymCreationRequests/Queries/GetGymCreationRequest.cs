using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Security;
using FitPass.Application.Extensions;
using FitPass.Application.Requests.DTOs;
using FitPass.Domain.Constants;

namespace Fitpass.Application.GymCreationRequests.Queries;

[Authorize(Roles = Roles.AppAdministrator)]
public record GetGymCreationRequestQuery(string GymCreationRequestId) : IRequest<GymCreationRequestDto?>;

public class GetGymCreationRequestQueryValidator : AbstractValidator<GetGymCreationRequestQuery>
{
    public GetGymCreationRequestQueryValidator()
    {
        RuleFor(v => v.GymCreationRequestId).NotEmptyWithMessage("Gym creation request id");
    }
}

public class GetGymCreationRequestQueryHandler : IRequestHandler<GetGymCreationRequestQuery, GymCreationRequestDto?>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetGymCreationRequestQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }
    public async Task<GymCreationRequestDto?> Handle(GetGymCreationRequestQuery request, CancellationToken cancellationToken)
    {
        var gymCreationRequest = await _context.GymCreationRequests.AsNoTracking().FirstOrDefaultAsync(gcr => gcr.Id == request.GymCreationRequestId);

        return _mapper.Map<GymCreationRequestDto>(gymCreationRequest);
    }
}