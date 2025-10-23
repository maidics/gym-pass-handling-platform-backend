using FitPass.Domain.Entities;
using FitPass.Domain.Enums;

namespace FitPass.Application.GymPassProducts.DTOs;

public class GymPassProductDto
{
    public required string GymId { get; set; }
    public required PassType Type { get; set; }
    public required int? TotalUses { get; set; }
    public required int? DaysAfterExpiring { get; set; }
    public required decimal HUFPrice { get; set; }
    public required bool IsActive { get; set; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<GymPassProduct, GymPassProductDto>();
        }
    }
}
