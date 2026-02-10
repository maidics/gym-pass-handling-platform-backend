using FitPass.Application.Common.Extensions;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Models;
using FitPass.Application.Common.Resources;
using FitPass.Application.GymPassProducts.DTOs;

namespace FitPass.Application.GymPassProducts.Queries;

public record GetGymPassProductByIdQuery(string GymPassProductId)
    : IRequest<Result<GymPassProductDto>>;

public class GetGymPassProductByIdQueryValidator : AbstractValidator<GetGymPassProductByIdQuery>
{
    public GetGymPassProductByIdQueryValidator(ILocalizer localizer)
    {
        RuleFor(v => v.GymPassProductId)
            .PropertyOfEntityNotEmptyWithMessageLocalized(
                localizer,
                nameof(SharedResource.Id),
                nameof(SharedResource.GymPassProduct)
            );
    }
}

public class GetGymPassProductByIdQueryHandler
    : IRequestHandler<GetGymPassProductByIdQuery, Result<GymPassProductDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ILocalizer _localizer;

    public GetGymPassProductByIdQueryHandler(IApplicationDbContext context, ILocalizer localizer)
    {
        _context = context;
        _localizer = localizer;
    }

    public async Task<Result<GymPassProductDto>> Handle(
        GetGymPassProductByIdQuery query,
        CancellationToken cancellationToken
    )
    {
        var dto = await _context
            .GymPassProducts.AsNoTracking()
            .Where(x => x.Id == query.GymPassProductId)
            .Select(x => new GymPassProductDto()
            {
                Id = x.Id,
                DaysAfterExpiring = x.DaysAfterExpiring,
                Description = x.Description,
                GymId = x.GymId,
                IsActive = x.IsActive,
                Name = x.Name,
                Price = x.Price,
                TotalUses = x.TotalUses,
                Type = x.Type,
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (dto is null)
        {
            return Result.NotFound(_localizer.GetNotFound(nameof(SharedResource.GymPassProduct)));
        }

        return Result.Success(dto);
    }
}
