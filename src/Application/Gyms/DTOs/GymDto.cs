using FitPass.Domain.Entities;
using FitPass.Domain.Enums;

namespace Fitpass.Application.Gyms.DTOs;

public class GymDto
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public required string Address { get; set; }
    public required GymStatus GymStatus { get; set; }
    public required GymTier GymTier { get; set; }
    public required DateTimeOffset CreationDate { get; set; }
    public string? OwnerName { get; set; }
    public List<GymPassProduct>? GymPassProducts { get; set; }
    public List<UserGymMembership>? UserGymMemberships { get; set; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Gym, GymDto>();
        }
    }
}