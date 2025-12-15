using FitPass.Application.Gyms.DTOs;
using FitPass.Application.Common.Extensions;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Security;
using FitPass.Domain.Constants;
using FitPass.Application.Common.Models;
using FitPass.Domain.Entities;

namespace FitPass.Application.Gyms.Queries;

[Authorize(Roles = $"{Roles.AppAdministrator}")]
public record GetGymByIdQuery(string GymId) : IRequest<Result<GymDto>>;

public class GetGymByIdQueryValidator : AbstractValidator<GetGymByIdQuery>
{
    public GetGymByIdQueryValidator()
    {
        RuleFor(v => v.GymId).NotEmptyLocalized(nameof(GetGymByIdQuery.GymId));
    }
}

public class GetGymByIdQueryHandler : IRequestHandler<GetGymByIdQuery, Result<GymDto>>
{
    private readonly IApplicationDbContext _context;

    public GetGymByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<GymDto>> Handle(GetGymByIdQuery query, CancellationToken cancellationToken)
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
