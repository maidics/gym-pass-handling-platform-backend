using FitPass.Domain.Entities;
using FitPass.Domain.Enums;
using FitPass.Domain.ValueObjects;

namespace FitPass.Application.GymPassProducts.DTOs;

public class GymPassProductDto
{
    public required string Id { get; set; }
    public required string GymId { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required PassType Type { get; set; }
    public required int? TotalUses { get; set; }
    public required int? DaysAfterExpiring { get; set; }
    public required Money Price { get; set; }
    public required bool IsActive { get; set; }
}

public static class Mappings
{
    extension(GymPassProduct gymPassProduct)
    {
        public GymPassProductDto MapToDto()
        {
            return new GymPassProductDto
            {
                Id = gymPassProduct.Id,
                GymId = gymPassProduct.GymId,
                Name = gymPassProduct.Name,
                Description = gymPassProduct.Description,
                Type = gymPassProduct.Type,
                TotalUses = gymPassProduct.TotalUses,
                DaysAfterExpiring = gymPassProduct.DaysAfterExpiring,
                Price = gymPassProduct.Price,
                IsActive = gymPassProduct.IsActive,
            };
        }
    }
}
