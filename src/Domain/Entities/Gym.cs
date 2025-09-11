namespace FitPass.Domain.Entities;

public class Gym : BaseEntity
{
    public required byte[] QRCode { get; set; }
    public required string Name { get; set; }
    public required string Address { get; set; }
    public string? OwnerName { get; set; }
    public ICollection<GymPassProduct> GymPassProducts { get; set; } = [];
    public ICollection<UserGymMembership> UserGymMemberships { get; set; } = [];
}