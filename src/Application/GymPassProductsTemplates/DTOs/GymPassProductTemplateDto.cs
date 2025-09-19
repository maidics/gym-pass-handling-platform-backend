using FitPass.Domain.Entities;
using FitPass.Domain.Enums;

namespace Fitpass.Application.GymPassProductsTemplates.DTOs;

public class GymPassProductTemplateDto
{
    public required string Id { get; set; }
    public required PassType PassType { get; set; }
    public required int? TotalUses { get; set; }
    public required int? DaysAfterExpiring { get; set; }
    public required decimal EurPrice { get; set; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<GymPassProductTemplate, GymPassProductTemplateDto>();
        }
    }
}