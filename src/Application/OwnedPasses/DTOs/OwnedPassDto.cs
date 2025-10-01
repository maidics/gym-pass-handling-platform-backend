using FitPass.Domain;
using FitPass.Domain.Enums;

namespace FitPass.Application.OwnedPasses.DTOs;

public class OwnedPassDto
{
    public required string Id { get; set; }
    public required string? UserGymMembershipId { get; set; }
    public required PassType Type { get; set; }
    public required int? TotalUses { get; set; }
    public required int? RemainingUses { get; set; }
    public required DateOnly? ExpirationDate { get; set; }
    public required decimal EurPrice { get; set; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<OwnedPass, OwnedPassDto>();
        }
    }
}
