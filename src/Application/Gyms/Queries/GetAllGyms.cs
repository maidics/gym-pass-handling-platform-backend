using FitPass.Application.Common.Interfaces;
using FitPass.Application.GymContactInfos.DTOs;
using FitPass.Application.GymPassProducts.DTOs;
using FitPass.Application.Gyms.DTOs;

namespace FitPass.Application.Gyms.Queries;

public record GetAllGymsQuery : IRequest<List<GymDto>>;

public class GetAllGymsQueryHandler : IRequestHandler<GetAllGymsQuery, List<GymDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAllGymsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<GymDto>> Handle(
        GetAllGymsQuery request,
        CancellationToken cancellationToken
    )
    {
        return await _context
            .Gyms.AsNoTracking()
            .Include(x => x.PassProducts)
            .Select(x => new GymDto()
            {
                Id = x.Id,
                Address = x.Address,
                ContactInfos = new List<GymContactInfoDto>(),
                PassProducts = x
                    .PassProducts.Select(y => new GymPassProductDto()
                    {
                        Id = y.Id,
                        Name = y.Name,
                        DaysAfterExpiring = y.DaysAfterExpiring,
                        Description = y.Description,
                        GymId = y.GymId,
                        IsActive = y.IsActive,
                        Price = y.Price,
                        TotalUses = y.TotalUses,
                        Type = y.Type,
                    })
                    .ToList(),
                CreatedBy = x.CreatedBy,
                CreatedOn = x.CreatedOn,
                LastModifiedBy = x.LastModifiedBy,
                LastModifiedOn = x.LastModifiedOn,
                Name = x.Name,
                PaymentProfile = null,
                Status = x.Status,
                Tier = x.Tier,
            })
            .ToListAsync(cancellationToken);
    }
}
