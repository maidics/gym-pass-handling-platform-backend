using FitPass.Application.Common.Extensions;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Models;
using FitPass.Application.GymPassProducts.DTOs;
using FitPass.Domain.Entities;

namespace FitPass.Application.GymPassProducts.Queries;

public record GetGymPassProductsByGymIdQuery(string GymId) : IRequest<Result<List<GymPassProductDto>>>;

public class GetGymPassProductsByGymIdQueryValidator : AbstractValidator<GetGymPassProductsByGymIdQuery>
{
    public GetGymPassProductsByGymIdQueryValidator()
    {
        RuleFor(v => v.GymId).NotEmptyWithMessage(nameof(GetGymPassProductsByGymIdQuery.GymId));
    }
}

public class GetGymPassProductsByGymIdQueryHandler : IRequestHandler<GetGymPassProductsByGymIdQuery, Result<List<GymPassProductDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetGymPassProductsByGymIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }
    public async Task<Result<List<GymPassProductDto>>> Handle(GetGymPassProductsByGymIdQuery query, CancellationToken cancellationToken)
    {
        var gym = await _context.Gyms
            .AsNoTracking()
            .Include(g => g.PassProducts)
            .FirstOrDefaultAsync(g => g.Id == query.GymId, cancellationToken);

        if (gym is null)
        {
            return Result.NotFound(nameof(Gym));
        }

        return Result.Success(gym.PassProducts.Select(p => p.MapToDto()).ToList());
    }
}