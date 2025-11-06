using FitPass.Application.GymPassProducts.DTOs;
using FitPass.Domain.Entities;
using FitPass.Domain.Enums;

namespace FitPass.Application.Gyms.DTOs;

public class GymDto
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public required string Address { get; set; }
    public required GymStatus Status { get; set; }
    public required GymTier Tier { get; set; }
    public required DateTimeOffset CreationDate { get; set; }
    public string? OwnerName { get; set; }
    public List<GymPassProductDto>? GymPassProducts { get; set; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Gym, GymDto>();
        }
    }
}
