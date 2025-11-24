using FitPass.Application.Common.Extensions;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Models;
using FitPass.Application.GymPassProducts.DTOs;

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
        var products = await _context.GymPassProducts
            .AsNoTracking()
            .Where(gpp => gpp.GymId == query.GymId)
            .ToListAsync();

        return Result.Success(products.Select(p => p.MapToDto()).ToList());
    }
}