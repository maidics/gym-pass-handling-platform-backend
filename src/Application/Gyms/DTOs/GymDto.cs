using FitPass.Domain.Entities;

namespace Fitpass.Application.Gyms.DTOs;

public class GymDto
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public required string Address { get; set; }
    public string? OwnerName { get; set; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Gym, GymDto>();
        }
    }
}