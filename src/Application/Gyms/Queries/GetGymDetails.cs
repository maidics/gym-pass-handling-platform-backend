using FitPass.Application.Gyms.DTOs;
using FitPass.Application.Common.Extensions;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Security;
using FitPass.Domain.Constants;
using FitPass.Application.Common.Models;
using FitPass.Domain.Entities;

namespace FitPass.Application.Gyms.Queries;

[Authorize(Roles = $"{Roles.AppAdministrator},{Roles.GymAdministrator},{Roles.GymStaff}")]
public record GetGymDetailsQuery(string GymId) : IRequest<Result<GymDto>>;

public class GetGymDetailsQueryValidator : AbstractValidator<GetGymDetailsQuery>
{
    public GetGymDetailsQueryValidator()
    {
        RuleFor(v => v.GymId).NotEmptyWithMessage(nameof(GetGymDetailsQuery.GymId));
    }
}

public class GetGymDetailsQueryHandler : IRequestHandler<GetGymDetailsQuery, Result<GymDto>>
{
    private readonly IApplicationDbContext _context;

    public GetGymDetailsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<GymDto>> Handle(GetGymDetailsQuery query, CancellationToken cancellationToken)
    {
        var gym = await _context
            .Gyms
            .AsNoTracking()
            .FirstOrDefaultAsync(g => g.Id == query.GymId, cancellationToken);

        if (gym is null)
        {
            return Result.NotFound(nameof(Gym));
        }

        return Result.Success(gym.MapToDto());
    }
}
