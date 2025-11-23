using FitPass.Application.Gyms.DTOs;
using FitPass.Application.Common.Extensions;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Security;
using FitPass.Domain.Constants;

namespace FitPass.Application.Gyms.Queries;

[Authorize(Roles = $"{Roles.AppAdministrator},{Roles.GymAdministrator},{Roles.GymStaff}")]
public record GetGymDetailsQuery(string GymId) : IRequest<GymDto>;

public class GetGymDetailsQueryValidator : AbstractValidator<GetGymDetailsQuery>
{
    public GetGymDetailsQueryValidator()
    {
        RuleFor(v => v.GymId).NotEmptyWithMessage(nameof(GetGymDetailsQuery.GymId));
    }
}

public class GetGymDetailsQueryHandler : IRequestHandler<GetGymDetailsQuery, GymDto>
{
    private readonly IApplicationDbContext _context;

    public GetGymDetailsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<GymDto> Handle(GetGymDetailsQuery query, CancellationToken cancellationToken)
    {
        var gym = await _context
            .Gyms
            .AsNoTracking()
            .FirstOrDefaultAsync(g => g.Id == query.GymId, cancellationToken);

        Guard.Against.NotFound(query.GymId, gym, "Id");

        return gym.MapToDto();
    }
}
