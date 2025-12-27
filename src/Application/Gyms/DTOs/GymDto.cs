using FitPass.Application.GymPassProducts.DTOs;
using FitPass.Domain.Entities;
using FitPass.Domain.Enums;
using FitPass.Domain.ValueObjects;

namespace FitPass.Application.Gyms.DTOs;

public class GymDto
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public required Address Address { get; set; }
    public required GymStatus Status { get; set; }
    public required GymTier Tier { get; set; }
    public required DateTimeOffset CreatedOn { get; set; }
    public List<GymPassProductDto> PassProducts { get; set; } = [];
}

public static class Mappings
{
    extension(Gym gym)
    {
        public GymDto MapToDto()
        {
            return new GymDto
            {
                Id = gym.Id,
                Name = gym.Name,
                Address = gym.Address,
                Status = gym.Status,
                Tier = gym.Tier,
                CreatedOn = gym.CreatedOn,
                PassProducts = [.. gym.PassProducts.Select(p => p.MapToDto())]
            };
        }
    }
}
