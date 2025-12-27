using FitPass.Application.Common.Extensions;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Models;
using FitPass.Application.GymPassProducts.DTOs;
using FitPass.Application.Common.Resources;

namespace FitPass.Application.GymPassProducts.Queries;

public record GetGymPassProductsByGymIdQuery(string GymId) : IRequest<Result<List<GymPassProductDto>>>;

public class GetGymPassProductsByGymIdQueryValidator : AbstractValidator<GetGymPassProductsByGymIdQuery>
{
    public GetGymPassProductsByGymIdQueryValidator(ILocalizer localizer)
    {
        RuleFor(v => v.GymId)
            .PropertyOfEntityNotEmptyWithMessageLocalized(localizer, nameof(SharedResource.Id), nameof(SharedResource.Gym));
    }
}

public class GetGymPassProductsByGymIdQueryHandler : IRequestHandler<GetGymPassProductsByGymIdQuery, Result<List<GymPassProductDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly ILocalizer _localizer;

    public GetGymPassProductsByGymIdQueryHandler(
        IApplicationDbContext context,
        ILocalizer localizer)
    {
        _context = context;
        _localizer = localizer;
    }
    public async Task<Result<List<GymPassProductDto>>> Handle(GetGymPassProductsByGymIdQuery query, CancellationToken cancellationToken)
    {
        var gym = await _context.Gyms
            .AsNoTracking()
            .Include(g => g.PassProducts)
            .FirstOrDefaultAsync(g => g.Id == query.GymId, cancellationToken);

        if (gym is null)
        {
            return Result.NotFound(_localizer.GetNotFound(nameof(SharedResource.Gym)));
        }

        return Result.Success(gym.PassProducts.Select(p => p.MapToDto()).ToList());
    }
}
